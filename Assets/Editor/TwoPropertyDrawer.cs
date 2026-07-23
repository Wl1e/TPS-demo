using UnityEditor;
using UnityEngine;

namespace TPSDemo.Editor
{

    public class TwoPropertyDrawer: PropertyDrawer
    {
        private SerializedProperty m_Property1;
        private SerializedProperty m_Property2;

        public virtual string Property1Name { get; protected set; }
        public virtual string Property2Name { get; protected set; }

        public virtual float NameWidthRatio { get; protected set; } = 0.3f;


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            m_Property1 = property.FindPropertyRelative(Property1Name);
            m_Property2 = property.FindPropertyRelative(Property2Name);

            float singleLineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = 5f;
            float nameWidth = (position.width - spacing) * NameWidthRatio;
            float clipWidth = position.width - spacing - nameWidth;
            Rect keyRect = new(position.x, position.y, nameWidth, singleLineHeight);
            Rect clipRect = new(position.x + nameWidth + spacing, position.y, clipWidth, singleLineHeight);

            EditorGUI.BeginProperty(position, label, property);

            EditorGUI.PropertyField(keyRect, m_Property1, GUIContent.none);
            EditorGUI.PropertyField(clipRect, m_Property2, GUIContent.none);

            EditorGUI.EndProperty();
        }
    }

    [CustomPropertyDrawer(typeof(AnimationEntry))]
    public class ManualAnimationDrawer: TwoPropertyDrawer
    {
        public override float NameWidthRatio { get { return 0.3f; } }
        public override string Property1Name { get { return "Name"; } }
        public override string Property2Name { get { return "Clip"; } }
    }

    [CustomPropertyDrawer(typeof(BulletItemData.TagEffect))]
    public class TagEffectDrawer : TwoPropertyDrawer
    {
        public override float NameWidthRatio { get { return 0.3f; } }
        public override string Property1Name { get { return "Tag"; } }
        public override string Property2Name { get { return "Effect"; } }
    }

    [CustomPropertyDrawer(typeof(SkillArgs))]
    public class SkillConfigDrawer : TwoPropertyDrawer
    {
        public override float NameWidthRatio { get { return 0.3f; } }
        public override string Property1Name { get { return "Key"; } }
        public override string Property2Name { get { return "Value"; } }
    }

    [CustomPropertyDrawer(typeof(SkillController.SkillEntry))]
    public class SkillEntryDrawer : TwoPropertyDrawer
    {
        public override float NameWidthRatio { get { return 0.3f; } }
        public override string Property1Name { get { return "Id"; } }
        public override string Property2Name { get { return "Config"; } }
    }
}
