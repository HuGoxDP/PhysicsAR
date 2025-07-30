using System;
using System.Collections;
using HuGox.Utils;
using UnityEngine;

namespace _Project.Scripts.Architecture.Scenario
{
    public class RopeSystem : MonoBehaviour
    {
        [SerializeField] private Transform[] _points;
        [SerializeField] private float _size;
        [SerializeField] private Color _ropeColor = Color.black;
        private bool _isRepinting = true;
        private LineRenderer _lineRenderer;

        private void Start()
        {
            _lineRenderer = this.GetComponentOrAdd<LineRenderer>();
            _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            _lineRenderer.positionCount = _points?.Length ?? 0;
            _lineRenderer.startWidth = _size;
            _lineRenderer.endWidth = _size;
            _lineRenderer.startColor = _lineRenderer.endColor = _ropeColor;
            _lineRenderer.sortingLayerName = "PreBack";
            UpdatePosition();
            StartCoroutine(UpdatePositionCoroutine(TimeSpan.FromMilliseconds(50), true));
        }

        private IEnumerator UpdatePositionCoroutine(TimeSpan delay, bool repeat = false)
        {
            float timeDelay = (float)delay.TotalSeconds;
            while (_isRepinting && repeat)
            {
                yield return new WaitForSeconds(timeDelay);
                UpdatePosition();
            }
        }

        private void UpdatePosition()
        {
            if (_points != null)
            {
                for (int i = 0; i < _points.Length; i++)
                {
                    _lineRenderer.SetPosition(i, _points[i].position);
                }
            }
        }
    }
}