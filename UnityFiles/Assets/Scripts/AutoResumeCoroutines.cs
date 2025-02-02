using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Auto-resumes coroutines that have been suspended,
/// for example, when a Scene is made inactive, then made active again
/// Thanks to https://answers.unity.com/questions/851707/onenableondisable-stop-and-resume-coroutines.html
/// </summary>

public abstract class AutoResumeCoroutines : MonoBehaviour
{
    private class Wrapper : IEnumerator
    {
        private IEnumerator m_Coroutine;
        private bool m_Alive = true;
        public bool IsAlive { get { return m_Alive; } }
        public object Current { get { return m_Coroutine.Current; } }

        public Wrapper(IEnumerator aCoroutine)
        {
            m_Coroutine = aCoroutine;
        }
        public bool MoveNext()
        {
            m_Alive = m_Coroutine.MoveNext();
            return m_Alive;
        }
        public void Reset()
        {
            throw new System.NotImplementedException();
        }
    }

    private List<Wrapper> m_Coroutines = new List<Wrapper>();
    public virtual void OnEnable()
    {
        ClearOld();
        for (int i = 0; i < m_Coroutines.Count; i++)
        {
            if (m_Coroutines[i].IsAlive)
                StartCoroutine(m_Coroutines[i]);
        }
    }

    public virtual void OnDisable()
    {
        for (int i = 0; i < m_Coroutines.Count; i++)
        {
            StopCoroutine(m_Coroutines[i]);
        }
    }

    private void ClearOld()
    {
        for (int i = m_Coroutines.Count - 1; i >= 0; i--)
        {
            if (!m_Coroutines[i].IsAlive)
                m_Coroutines.RemoveAt(i);
        }
    }

    public void StartAutoResumeCoroutine(IEnumerator aCoroutine)
    {
        var inst = new Wrapper(aCoroutine);
        m_Coroutines.Add(inst);
        StartCoroutine(inst);
        ClearOld();
    }
}