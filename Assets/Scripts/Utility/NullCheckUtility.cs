using System.Reflection;
using UnityEngine;

public static class NullCheckUtility
{
    public static void CheckForNullFields(object obj)
    {
        if (obj == null)
        {
            Debug.LogError("Object is null");
            return;
        }

        FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

        foreach (FieldInfo field in fields)
        {
            object value = field.GetValue(obj);

            if (value == null)
            {
                Debug.LogError($"{obj.GetType().Name} missing reference: {field.Name}");
                continue;
            }

            if (value is UnityEngine.Object unityObj && unityObj == null)
            {
                Debug.LogError($"{obj.GetType().Name} missing reference: {field.Name}");
            }
        }
    }
}
