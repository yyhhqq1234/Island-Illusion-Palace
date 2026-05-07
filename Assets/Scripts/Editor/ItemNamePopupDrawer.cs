using UnityEngine;

// ItemNamePopupAttribute 类定义在全局命名空间，这样在非编辑器模式下也能找到
public class ItemNamePopupAttribute : PropertyAttribute
{
}

#if UNITY_EDITOR
// ItemNamePopupDrawer 类定义需要添加到Editor命名空间
namespace UnityEditor
{
    using System.Collections.Generic;
    using System.Linq;
    
    [CustomPropertyDrawer(typeof(ItemNamePopupAttribute))]
    public class ItemNamePopupDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }
            
            List<string> options = GetOptionsForProperty(property);
            
            if (options == null || options.Count == 0)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }
            
            int currentIndex = options.IndexOf(property.stringValue);
            if (currentIndex < 0) currentIndex = 0;
            
            label = EditorGUI.BeginProperty(position, label, property);
            
            Rect popupRect = new Rect(position.x, position.y, position.width, position.height);
            
            int newIndex = EditorGUI.Popup(popupRect, label.text, currentIndex, options.ToArray());
            
            if (newIndex >= 0 && newIndex < options.Count)
            {
                property.stringValue = options[newIndex];
            }
            
            EditorGUI.EndProperty();
        }
        
        private List<string> GetOptionsForProperty(SerializedProperty property)
        {
            string path = property.propertyPath;
            
            // 解析属性路径确定分类
            // 路径格式示例：
            // iconDatabase.materials.icons.Array.data[0].itemName
            // iconDatabase.consumables.icons.Array.data[5].itemName  
            // iconDatabase.weapons.icons.Array.data[2].itemName
            
            if (path.Contains(".materials.") || path.Contains(".materials"))
            {
                Debug.Log($"[ItemSlot] 检测到材料分类: {path}");
                return ItemSlot.GetAllMaterialNames();
            }
            else if (path.Contains(".consumables.") || path.Contains(".consumables"))
            {
                Debug.Log($"[ItemSlot] 检测到消耗品分类: {path}");
                return ItemSlot.GetAllConsumableNames();
            }
            else if (path.Contains(".weapons.") || path.Contains(".weapons"))
            {
                Debug.Log($"[ItemSlot] 检测到武器分类: {path}");
                return ItemSlot.GetAllWeaponNames();
            }
            
            // 备用方案：通过属性路径中的关键字判断
            Debug.Log($"[ItemSlot] 使用备用检测: {path}");
            
            if (path.ToLower().Contains("material"))
                return ItemSlot.GetAllMaterialNames();
            else if (path.ToLower().Contains("consumable"))
                return ItemSlot.GetAllConsumableNames();
            else if (path.ToLower().Contains("weapon"))
                return ItemSlot.GetAllWeaponNames();
            
            // 最终默认返回材料列表
            return ItemSlot.GetAllMaterialNames();
        }
    }
}
#endif