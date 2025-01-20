using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace multiplayerstew.Scripts.Attributes
{
    public class AnimationsRequiredAttribute : Attribute
    {
        public string[] AnimationNames;
        public AnimationsRequiredAttribute(string[] animationNames) 
        { 
            AnimationNames = animationNames;
        }
    }
}
