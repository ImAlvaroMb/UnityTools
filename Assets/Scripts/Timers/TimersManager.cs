using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class TimersManager : MonoBehaviour
{
    private LinkedList<Timer> timersPool = new LinkedList<Timer>();
    private LinkedList<Timer> activeTimers = new LinkedList<Timer>();
    private Dictionary<string, Timer> accessibleTimers = new Dictionary<string, Timer>();
    [SerializeField] private int maxPoolSize;
    [SerializeField] private int initalPoolSize;

    public static TimersManager Instance { get; private set; }

    public class Timer
    {
        public float duration;
        public float elapsedTime;
        public Action callback;
        public bool isRepating;
        public string id;

        public Timer(float duration, Action callback, bool isRepating, string id)
        {
            Reset(duration, callback, isRepating, id);
        }

        public void Reset(float duration, Action callback, bool isRepating, string id)
        {
            this.duration = duration;
            elapsedTime = 0;
            this.callback = callback;
            this.isRepating = isRepating;
            this.id = id;
        }

        public float getProgress()
        {
            return duration > 0 ? duration / elapsedTime : 1f;
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePool();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializePool()
    {
        for (int i = 0; i < initalPoolSize; i++)
        {
            var Timer = new Timer(0, null, false, "");
            timersPool.AddLast(Timer);
        }
    }

    public Timer StartTimer(float duration, Action callback, string id, bool isRepating = false, bool isAccessible = false)
    {
        Timer timer;
        if(isAccessible && accessibleTimers.ContainsKey(id))
        {
            Debug.LogWarning($"A timer with the identifier : {id} already exists");
            return null;
        } 

        if(timersPool.Count > 0) // there is aveliable timers in the opool
        {
            var node = timersPool.First;
            timer = node.Value;
            timersPool.RemoveFirst();
            timer.Reset(duration, callback, isRepating, id);
        } else if (timersPool.Count + 1 < maxPoolSize) // there are no aveliable timers on the pool creates a new one if it doesnt exceed the maxLimit
        {
            timer = new Timer(duration, callback, isRepating, id);
            timersPool.AddLast(timer);
            Debug.Log($"Expanding pool size to {timersPool.Count + 1}");
        } else
        {
            Debug.LogWarning($"Timer pool size reached its limit: {maxPoolSize}");
            return null;
        }

        if (isAccessible && !accessibleTimers.ContainsKey(id))
        {
            accessibleTimers.Add(id, timer);
        }
        activeTimers.AddLast(timer);
        return timer;
    }

    public void StopTimer(string id)
    {
        if(accessibleTimers.TryGetValue(id, out var timer))
        {
            RemoveTimer(timer);
        } else
        {
            Debug.LogWarning($"{id} is not accessible");
        }
    }

    private void RemoveTimer(Timer timer)
    {
        activeTimers.Remove(timer);
        if(accessibleTimers.ContainsKey(timer.id))
        {
            accessibleTimers.Remove(timer.id);
        }
        timersPool.AddLast(timer);
    }

    private void Update()
    {
        foreach(var timer in new LinkedList<Timer>(activeTimers)) // create a copy to ensure safety while iteratoin over the list
        {
            timer.elapsedTime += Time.deltaTime;

            if(timer.elapsedTime >= timer.duration)
            {
                timer.callback?.Invoke(); // the ? (non-conditional operator) this expression evaluates to null if no callback has been assigned to prevent NullReferences
                if(timer.isRepating)
                {
                    timer.elapsedTime = 0;
                } else 
                {
                    RemoveTimer(timer);
                }
            }
        }
    }

    private void OnDestroy()
    {
        foreach(var timer in activeTimers)
        {
            RemoveTimer(timer);
        }
        
    }

    /*TimersManager.Instance.StartTimer(5f, () =>
    {
        fucntion to be called or code to be executed
        could add more statements here if necessary
    }, "collectItemTimer");*/

    /*TimersManager.Instance.StartTimer(3f, () => 
    {
        functions
    }, id, true (repeating), true (uses id));*/

}
