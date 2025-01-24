using Godot;
using multiplayerstew.Scripts.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace multiplayerstew.Scripts.Services
{
    public static class GodotErrorService
    {
        public static void ValidateRequiredData(Node node)
        {
            // Only run in editor. No need to run possibly expensive code outside of editor
            if (OS.HasFeature("editor"))
            {
                Type type = node.GetType();
                IEnumerable<PropertyInfo> properties = type.GetProperties();
                // In the passed type, get all custom attributes and see if our custom attributes are found
                foreach (PropertyInfo prop in properties)
                {
                    // ExportRequiredAttribute
                    if (Attribute.GetCustomAttributes(prop).Where(a => a is ExportRequiredAttribute).Any())
                    {
                        if (prop.GetValue(node) == null)
                            GD.PushError($"Export {node.Name}:{prop.Name} is not assigned in {type.Name}");
                        else if(prop.PropertyType.IsArray && (prop.GetValue(node) as Array).Length == 0)
                            GD.PushError($"Export {prop.Name} in {node.Name}:{type.Name} is Empty");

                    }

                    // AnimationsRequiredAttribute
                    string[] animationNames = (Attribute.GetCustomAttributes(prop).Where(a => a is AnimationsRequiredAttribute)?.FirstOrDefault() as AnimationsRequiredAttribute)?.AnimationNames ?? Array.Empty<string>();
                    if (animationNames.Any())
                    {
                        AnimationPlayer APlayer = prop.GetValue(node) as AnimationPlayer;
                        foreach (string name in animationNames)
                        {
                            if (!APlayer.HasAnimation(name))
                                GD.PushError($"{prop.Name} in {node.Name}:{type.Name} is missing a '{name}' animation");
                        }
                    }

                }

            }
        }
    }
}
