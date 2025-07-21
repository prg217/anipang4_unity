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
        // �����ؽ�ó ����
        m_renderTexture = new RenderTexture(m_width, m_height, 24);
        m_renderTexture.Create();

        // ī�޶� ����
        m_captureCamera.targetTexture = m_renderTexture;
    }

    public List<Texture2D> Capture()
    {
        // �ӽ������� ī�޶� ������
        m_captureCamera.Render();

        // RenderTexture���� Texture2D�� ����
        RenderTexture.active = m_renderTexture;
        Texture2D tex = new Texture2D(m_width, m_height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, m_width, m_height), 0, 0);
        tex.Apply();

        // List�� ����
        m_captureTextures.Add(tex);

        // ����
        RenderTexture.active = null;

        return m_captureTextures;
    }
}
