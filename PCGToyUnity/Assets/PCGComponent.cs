using CatSAT;
using UnityEngine;

/// <summary>
/// A MonoBehavior that automatically fills in its own fields based on a PCGToy file
/// </summary>
public abstract class PCGComponent : MonoBehaviour
{
    static PCGComponent()
    {
        CatSAT.Random.SetSeed();
    }

    public string GeneratorName;

    protected PCGComponent()
    {
        // Default generator name
        GeneratorName = GetType().Name;
    }

	internal void OnEnable()
	{
        var generator = PCGToy.Generator.SharedFromFile(System.IO.Path.Combine(Application.dataPath, GeneratorName + ".scm"));
        generator.Populate(this);
	}
}
