using UnityEngine;

[CreateAssetMenu(fileName = "Enemy Preset", menuName = "New Enemy Preset")]
public class EnemyPreset : ScriptableObject{
    public string _name;
    public int id;
    public Sprite sprite;
}