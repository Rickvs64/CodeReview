using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuizLivesIndicator : MonoBehaviour
{
    [SerializeField]
    List<MeshRenderer> meshes;
    [SerializeField]
    Material availableMaterial;
    [SerializeField]
    Material consumedMaterial;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Reflect the current amount of quiz lives/"chances" in the scene.
    /// In the current state this means illuminating the amount of available meshes, while consumed lives will turn dark.
    /// </summary>
    /// <param name="lives"></param>
    public void ShowLives(int lives)
    {
        int i = 0;
        foreach (MeshRenderer m in meshes)
        {
            if (i < lives)
                m.material = availableMaterial;
            else
                m.material = consumedMaterial;

            i++;
        }
    }
}
