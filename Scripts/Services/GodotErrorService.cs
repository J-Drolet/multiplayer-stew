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
        public static void ValidateRequiredData(Object obj)
        {
            Type type = obj.GetType();
            IEnumerable<PropertyInfo> properties = type.GetProperties();
            // In the passed type, get all custom attributes and see if our custom attributes are found
            foreach (PropertyInfo prop in properties)
            {
                // ExportRequiredAttribute
                if (Attribute.GetCustomAttributes(prop).Where(a => a is ExportRequiredAttribute).Any())
                {
                    if(prop.GetValue(obj) == null)
                        GD.PushError($"Export {prop.Name} is not assigned in {type.Name}");
                }

                // AnimationsRequiredAttribute
                string[] animationNames = (Attribute.GetCustomAttributes(prop).Where(a => a is AnimationsRequiredAttribute)?.FirstOrDefault() as AnimationsRequiredAttribute)?.AnimationNames ?? Array.Empty<string>();
                if (animationNames.Any())
                {
                    AnimationPlayer APlayer = prop.GetValue(obj) as AnimationPlayer;
                    foreach (string name in animationNames)
                    {
                        if(!APlayer.HasAnimation(name))
                            GD.PushError($"{prop.Name} is missing a '{name}' animation");
                    }
                }

            }
        }
    }
}
