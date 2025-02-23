using TMPro;
using UnityEngine;
using System;

public class TimerImage : MonoBehaviour
{
    [SerializeField]
    private string baseText = "Clockout Available In: ";
    
    [SerializeField]
    private TextMeshProUGUI timerText;
    
    [SerializeField]
    private Color timerDoneColor = Color.red;
    
    public event Action OnTimerDone;


    private float _timeLeft;

    private bool _isCountingDown;

    private void Awake()
    {
        timerText.text = baseText + "0.00";
        
        _timeLeft = 0;
        
        _isCountingDown = false;
        
        timerText  = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Update()
    {
        if (_isCountingDown && _timeLeft > 0)
        {
            _timeLeft -= Time.deltaTime;
            
            timerText.text = baseText + _timeLeft.ToString("F2");

            if (_timeLeft <= 0)
            {
                _isCountingDown = false;

                timerText.text = baseText + "0.00";

                timerText.color = timerDoneColor;

                OnTimerDone?.Invoke();
            }
        }
        
    }
    
    public void SetAndStartTimer(float time)
    {
        _isCountingDown = true;
        
        _timeLeft = time;
    }
}
