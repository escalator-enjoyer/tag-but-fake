using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillWall : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        RunnerAgent runner = collision.gameObject.GetComponent<RunnerAgent>();
        TaggerAgent tagger = collision.gameObject.GetComponent<TaggerAgent>();
        if (runner)
        {
            runner.AddReward(-100f);
        }
        if (tagger)
        {
            tagger.AddReward(-100f);
        }
    }
}
