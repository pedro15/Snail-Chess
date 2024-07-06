using System.Reflection;

namespace SnailChess.Cmd
{
    public static class JITUtility
    {
        public static void InitializeJIT(Assembly asm)
        {
            System.Type[] types = asm.GetTypes();
            foreach (var type in types)
            {
                foreach (var method in type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | 
                BindingFlags.Static | BindingFlags.GetProperty))
                {
                    if ((method.Attributes & MethodAttributes.Abstract) == MethodAttributes.Abstract || method.ContainsGenericParameters)
                    {
                        continue;
                    }
                    
                    try 
                    {
                        System.Runtime.CompilerServices.RuntimeHelpers.PrepareMethod(method.MethodHandle);
                    }catch { continue; }
                }
            }
        }
    }
}