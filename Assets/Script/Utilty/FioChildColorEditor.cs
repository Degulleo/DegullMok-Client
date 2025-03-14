using UnityEngine;
using System.Collections.Generic;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Linq;
#endif

// 하위 요소들의 색상을 인스펙터에서 수정할 수 있게 해주는 컴포넌트
public class FioChildColorEditor : MonoBehaviour
{
    // 인스펙터에서 수정할 색상 정보
    [System.Serializable]
    public class ColorSettings
    {
        public GameObject targetObject; // 직접 오브젝트 참조
        public string componentType; // 컴포넌트 타입 (읽기 전용)
        public Color color = Color.white; // 적용할 색상
        public bool applyToChildren = false; // 해당 요소의 모든 하위 요소에도 적용할지 여부

        [HideInInspector]
        public Component colorableComponent; // 색상을 적용할 컴포넌트 참조
    }

    // 인스펙터에서 편집할 색상 목록
    public List<ColorSettings> colorSettings = new List<ColorSettings>();

    // 자동 감지 옵션
    public bool autoDetectChildren = true;

    // 지원되는 컴포넌트 유형들
    private static readonly System.Type[] supportedTypes = new System.Type[]
    {
        typeof(Renderer),
        typeof(SpriteRenderer),
        typeof(UnityEngine.UI.Image),
        typeof(UnityEngine.UI.RawImage),
        typeof(UnityEngine.UI.Text),
        typeof(TMP_Text)
    };

    // 초기화 시 하위 요소 자동 감지
    private void OnValidate()
    {
        if (autoDetectChildren)
        {
            DetectChildComponents();
        }

        // 모든 항목에 대해 컴포넌트 타입 업데이트
        UpdateComponentTypes();
    }

    // 하위 요소 자동 감지
    public void DetectChildComponents()
    {
        // 현재 목록을 유지할 오브젝트들
        HashSet<GameObject> existingObjects = new HashSet<GameObject>();
        foreach (var setting in colorSettings)
        {
            if (setting.targetObject != null)
            {
                existingObjects.Add(setting.targetObject);
            }
        }

        // 모든 자식 오브젝트 검색
        List<ColorSettings> newSettings = new List<ColorSettings>();
        foreach (Transform child in transform)
        {
            // 이미 목록에 있는 오브젝트는 건너뛰기
            if (existingObjects.Contains(child.gameObject))
            {
                continue;
            }

            // 지원되는 컴포넌트 찾기
            Component colorableComponent = FindColorableComponent(child.gameObject);
            if (colorableComponent != null)
            {
                ColorSettings setting = new ColorSettings
                {
                    targetObject = child.gameObject,
                    colorableComponent = colorableComponent,
                    color = GetCurrentColor(colorableComponent)
                };
                newSettings.Add(setting);
            }
        }

        // 새로운 항목 추가 (기존 항목은 유지)
        colorSettings.AddRange(newSettings);

        // 컴포넌트 타입 업데이트
        UpdateComponentTypes();
    }

    // 컴포넌트 타입 문자열 업데이트
    private void UpdateComponentTypes()
    {
        foreach (var setting in colorSettings)
        {
            if (setting.targetObject != null)
            {
                // 컴포넌트 찾기
                Component comp = setting.colorableComponent;
                if (comp == null)
                {
                    comp = FindColorableComponent(setting.targetObject);
                    setting.colorableComponent = comp;
                }

                if (comp != null)
                {
                    setting.componentType = GetComponentTypeName(comp);
                }
                else
                {
                    setting.componentType = "지원되지 않음";
                }
            }
            else
            {
                setting.componentType = "없음";
            }
        }
    }

    // 컴포넌트 타입 이름 얻기
    private string GetComponentTypeName(Component component)
    {
        if (component is Renderer) return "Renderer";
        if (component is SpriteRenderer) return "SpriteRenderer";
        if (component is UnityEngine.UI.Image) return "Image";
        if (component is UnityEngine.UI.RawImage) return "RawImage";
        if (component is UnityEngine.UI.Text) return "Text";
        if (component is TMP_Text) return "TMP_Text";
        return component.GetType().Name;
    }

    // 색상을 적용할 수 있는 컴포넌트 찾기
    private Component FindColorableComponent(GameObject obj)
    {
        foreach (var type in supportedTypes)
        {
            Component comp = obj.GetComponent(type);
            if (comp != null)
            {
                return comp;
            }
        }
        return null;
    }

    // 현재 색상 가져오기
    private Color GetCurrentColor(Component component)
    {
        if (component is Renderer renderer)
        {
            return renderer.material.color;
        }
        else if (component is SpriteRenderer spriteRenderer)
        {
            return spriteRenderer.color;
        }
        else if (component is UnityEngine.UI.Graphic graphic)
        {
            return graphic.color;
        }
        else if (component is TMP_Text tmpText)
        {
            return tmpText.color;
        }

        return Color.white;
    }

    // 색상을 적용하는 함수
    public void ApplyColors()
    {
        foreach (var setting in colorSettings)
        {
            if (setting.targetObject != null)
            {
                ApplyColorToObject(setting.targetObject, setting.color, setting.applyToChildren);
            }
        }
    }

    // 특정 오브젝트에 색상 적용
    private void ApplyColorToObject(GameObject obj, Color color, bool applyToChildren)
    {
        // 렌더러
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = color;
        }

        // 스프라이트 렌더러
        SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
        }

        // UI 이미지
        UnityEngine.UI.Image image = obj.GetComponent<UnityEngine.UI.Image>();
        if (image != null)
        {
            image.color = color;
        }

        // UI 로우 이미지
        UnityEngine.UI.RawImage rawImage = obj.GetComponent<UnityEngine.UI.RawImage>();
        if (rawImage != null)
        {
            rawImage.color = color;
        }

        // UI 텍스트
        UnityEngine.UI.Text text = obj.GetComponent<UnityEngine.UI.Text>();
        if (text != null)
        {
            text.color = color;
        }

        // TextMeshPro 텍스트
        TMP_Text tmpText = obj.GetComponent<TMP_Text>();
        if (tmpText != null)
        {
            tmpText.color = color;
        }

        // 하위 요소에도 재귀적으로 색상 적용
        if (applyToChildren)
        {
            foreach (Transform child in obj.transform)
            {
                ApplyColorToObject(child.gameObject, color, true);
            }
        }
    }
}

#if UNITY_EDITOR
// 에디터 인스펙터 커스터마이징
[CustomEditor(typeof(FioChildColorEditor))]
public class ChildColorEditorInspector : Editor
{
    private SerializedProperty colorSettingsProperty;
    private SerializedProperty autoDetectChildrenProperty;

    private void OnEnable()
    {
        colorSettingsProperty = serializedObject.FindProperty("colorSettings");
        autoDetectChildrenProperty = serializedObject.FindProperty("autoDetectChildren");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        FioChildColorEditor colorEditor = (FioChildColorEditor)target;

        // 자동 감지 옵션
        EditorGUILayout.PropertyField(autoDetectChildrenProperty, new GUIContent("자동 하위 요소 감지"));

        // 새로고침 버튼
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("하위 요소 새로 감지", GUILayout.Height(30)))
        {
            colorEditor.DetectChildComponents();
            serializedObject.Update();
        }

        if (GUILayout.Button("목록 초기화", GUILayout.Height(30)))
        {
            colorSettingsProperty.ClearArray();
            colorEditor.DetectChildComponents();
            serializedObject.Update();
        }
        EditorGUILayout.EndHorizontal();

        // 구분선 추가
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        // 목록 타이틀
        EditorGUILayout.LabelField("하위 요소 색상 설정", EditorStyles.boldLabel);

        // 각 항목 표시
        for (int i = 0; i < colorSettingsProperty.arraySize; i++)
        {
            SerializedProperty itemProperty = colorSettingsProperty.GetArrayElementAtIndex(i);
            SerializedProperty targetObjectProperty = itemProperty.FindPropertyRelative("targetObject");
            SerializedProperty componentTypeProperty = itemProperty.FindPropertyRelative("componentType");
            SerializedProperty colorProperty = itemProperty.FindPropertyRelative("color");
            SerializedProperty applyToChildrenProperty = itemProperty.FindPropertyRelative("applyToChildren");

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // 왼쪽에 오브젝트 선택 필드, 오른쪽에 컴포넌트 타입 표시
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(targetObjectProperty, GUIContent.none, GUILayout.ExpandWidth(true));

            GUI.enabled = false;
            EditorGUILayout.TextField(componentTypeProperty.stringValue, GUILayout.Width(100));
            GUI.enabled = true;

            if (GUILayout.Button("✕", GUILayout.Width(20)))
            {
                colorSettingsProperty.DeleteArrayElementAtIndex(i);
                serializedObject.ApplyModifiedProperties();
                break;
            }
            EditorGUILayout.EndHorizontal();

            // 색상과 하위 요소 적용 옵션
            EditorGUILayout.PropertyField(colorProperty);
            EditorGUILayout.PropertyField(applyToChildrenProperty, new GUIContent("하위 요소에도 적용"));

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        // 항목 추가 버튼
        if (GUILayout.Button("항목 추가"))
        {
            colorSettingsProperty.arraySize++;
            serializedObject.ApplyModifiedProperties();
        }

        // 구분선 추가
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space();

        // 색상 적용 버튼 추가
        if (GUILayout.Button("모든 색상 적용하기", GUILayout.Height(40)))
        {
            colorEditor.ApplyColors();

            // 씬 변경 사항 저장
            EditorUtility.SetDirty(target);
            if (!Application.isPlaying)
            {
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif