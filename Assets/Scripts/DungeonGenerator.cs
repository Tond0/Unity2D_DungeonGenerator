using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Tilemaps;
using static System.Collections.Specialized.BitVector32;

public class DungeonGenerator : MonoBehaviour
{
    [Header("Dungeon alghoritms")]
    [SerializeField] private Generator generator;

    [Header("Debug")]
    [SerializeField] private bool generateAgain;

    private void Start()
    {
        generator.Generate(transform.position);
    }


    private void LateUpdate()
    {
        if (generateAgain)
            //FIXME: Da mettere lo stesso ombrello tutti i vari tipi di gen
            generator.Generate(transform.position);

        generateAgain = false;
    }
}
