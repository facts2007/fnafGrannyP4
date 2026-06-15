using UnityEngine;
using UnityEngine.Playables;

public class TimelineController : MonoBehaviour
{
    public PlayableDirector director;

    public void PlayTimeline()
    {
        director.Stop();
        director.time = 0;
        director.Evaluate();
        director.Play();
    }
}