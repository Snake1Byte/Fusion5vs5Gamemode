using System.Reflection;
using BoneLib.BoneMenu.Elements;

namespace Fusion5vs5Gamemode.Utilities.Extensions;

public static class IntElementExtensions
{
    public static void SetIncrement(this IntElement a, int b)
    {
        var type = a.GetType();
        var field = type.GetField("_increment", BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(a, b);
        }
    }
}