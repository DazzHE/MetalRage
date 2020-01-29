using UnityEngine;
using System.Collections;
using System.Linq;

[CreateAssetMenu(fileName = "GameConfig", menuName = "MetalRage/GameConfig")]
public class GameConfig : ScriptableObject {
    [SerializeField]
    private ScriptableObject[] configs = default;

    public T GetConfig<T>() where T : ScriptableObject {
        return this.configs.Where(c => c.GetType() == typeof(T)).FirstOrDefault() as T;
    }
}