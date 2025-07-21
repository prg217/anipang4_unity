using System.Collections.Generic;
using UnityEngine;

public class CaptureCamera : MonoBehaviour
{
    [SerializeField]
    Camera m_captureCamera;
    int m_width = 850;
    int m_height = 850;

    RenderTexture m_renderTexture;
    List<Texture2D> m_captureTextures = new List<Texture2D>();

    void Start()
    {
        // 렌더텍스처 생성
        m_renderTexture = new RenderTexture(m_width, m_height, 24);
        m_renderTexture.Create();

        // 카메라에 연결
        m_captureCamera.targetTexture = m_renderTexture;
    }

    public List<Texture2D> Capture()
    {
        // 임시적으로 카메라를 렌더링
        m_captureCamera.Render();

        // RenderTexture에서 Texture2D로 복사
        RenderTexture.active = m_renderTexture;
        Texture2D tex = new Texture2D(m_width, m_height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, m_width, m_height), 0, 0);
        tex.Apply();

        // List에 저장
        m_captureTextures.Add(tex);

        // 정리
        RenderTexture.active = null;

        return m_captureTextures;
    }
}
