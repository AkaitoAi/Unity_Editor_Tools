using UnityEngine;

[CreateAssetMenu(fileName = "TextureMaterial", menuName = "ScriptableObjects/TextureMaterial", order = 1)]
public class TextureMaterialSO : ScriptableObject
{
    public Texture2D texture2D;
    public Material material;
}
