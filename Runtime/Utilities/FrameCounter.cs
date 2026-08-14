using System;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace Ulys.Runtime.Utilities
{

public class FrameCounter : MonoBehaviour
{
	[Header("Properties")]
	[SerializeField] private float updateFrequency = 0.1f;
	[SerializeField, Range(1, 1024)] private int savedDataFrames = 128;
	[SerializeField, Range(0, 200)] private int targetFrameRate = 0;
	
	[Header("Screen Text")]
	[SerializeField] private TextPlacement textPlacement = TextPlacement.TopRight;
	[SerializeField] private int fontSize = 24;
	[SerializeField] private Color textColor = Color.white;
	[SerializeField] private Color backgroundColor = new(0f, 0f, 0f, 0.5f);
	
	[Header("Events")]
	[SerializeField] private UnityEvent<float> onFPSValueChanged;
	[SerializeField] private UnityEvent<float> onAverageValueChanged;

	private float _frameTime;
	private float _fps;
	private float _averageFrameTime;
	private float _averageFPS;
	private float _updateCooldown;
	private float _lastUpdateTime;
	private int _sampleCount;
	private float _totalFrameTime;

	private StringBuilder _stringBuilder;
	
	private int _ringIndex;
	private float[] _ringBuffer;

	private Rect _textRect;
	private GUIStyle _style;
	private GUIContent _content;
	private Texture2D _backgroundTexture;

	private void Start()
	{
		_updateCooldown = 1f / updateFrequency;
		_totalFrameTime = 0f;
		_averageFrameTime = 0f;
		_ringIndex = 0;
		_ringBuffer = new float[savedDataFrames];
		_lastUpdateTime = Time.unscaledTime;

		_stringBuilder = new(256);
		
		_style = new()
		{
			fontStyle = FontStyle.Bold,
		};
		_content = new();
		_backgroundTexture = new(1, 1);
		_backgroundTexture.SetPixel(0, 0, backgroundColor);
		_backgroundTexture.Apply();
		_style.normal.background = _backgroundTexture;
		_style.padding = new RectOffset(10, 10, 10, 10);
	}

	private void UpdateStyle()
	{
		if (!Application.isPlaying)
			return;
		
		Vector2 size = _style.CalcSize(_content);
		
		TextAnchor anchor = TextAnchor.MiddleCenter;
		float width = size.x;
		float height = size.y;
		float padding = 10f;
		_textRect = new(0, 0, width, height);
		
		switch (textPlacement)
		{
		case TextPlacement.None:
			break;
		case TextPlacement.TopLeft:
			anchor = TextAnchor.UpperLeft;
			_textRect.x = padding;
			_textRect.y = padding;
			break;
		case TextPlacement.TopRight:
			anchor = TextAnchor.UpperRight;
			_textRect.x = Screen.width - (width + padding);
			_textRect.y = padding;
			break;
		case TextPlacement.BottomLeft:
			anchor = TextAnchor.LowerLeft;
			_textRect.x = padding;
			_textRect.y = Screen.height - (height + padding);
			break;
		case TextPlacement.BottomRight:
			anchor = TextAnchor.LowerRight;
			_textRect.x = Screen.width - (width + padding);
			_textRect.y = Screen.height - (height + padding);
			break;
		default:
			break;
		}
		
		_style.alignment = anchor;
		_style.fontSize = fontSize * Screen.height / 1080;
		_style.normal.textColor = textColor;
		
		_backgroundTexture.SetPixel(0, 0, backgroundColor);
		_backgroundTexture.Apply();
		_style.normal.background = _backgroundTexture;
	}
	
	private void OnGUI()
	{
		if (textPlacement == TextPlacement.None)
			return;

		if (Event.current.type != EventType.Repaint)
			return;

		UpdateStyle();
		
		GUI.Label(
			_textRect,
			_content,
			_style
		);
	}

	private void Update()
	{
		if (targetFrameRate > 1)
			Application.targetFrameRate = targetFrameRate;
		else
			Application.targetFrameRate = -1;
		
		_frameTime = Time.unscaledDeltaTime;
		_fps = 1.0f / _frameTime;
		
		_totalFrameTime -= _ringBuffer[_ringIndex];
		_ringBuffer[_ringIndex] = _frameTime;
		_ringIndex = (_ringIndex + 1) % _ringBuffer.Length;
		
		_totalFrameTime += _frameTime;
		_sampleCount = Mathf.Min(_sampleCount + 1, _ringBuffer.Length);
		_averageFrameTime = _totalFrameTime / _sampleCount;
		_averageFPS = 1f / _averageFrameTime;
		
		if (Time.unscaledTime - _lastUpdateTime > _updateCooldown)
		{
			_lastUpdateTime = Time.unscaledTime;
			onFPSValueChanged?.Invoke(_fps);
			onAverageValueChanged?.Invoke(_averageFPS);
			
			// Text building
			_stringBuilder.Clear();
			_stringBuilder.AppendLine($"FPS: {_fps:F1}");
			_stringBuilder.Append($"AVG: {_averageFPS:F1}");
			//_stringBuilder.Append($"WAT: {0f:F1}");
			
			_content.text = _stringBuilder.ToString();
		}
	}
	
	private enum TextPlacement
	{
		None,
		TopLeft,
		TopRight,
		BottomLeft,
		BottomRight,
	}
}

}
