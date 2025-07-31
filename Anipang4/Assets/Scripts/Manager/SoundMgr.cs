using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundMgr : BaseMgr<SoundMgr>
{
    [SerializeField]
    List<SBGMData> m_BGMDatas;
    [SerializeField]
    List<SSFXData> m_SFXDatas;

    Dictionary<EBGM, AudioClip> m_bgms = new Dictionary<EBGM, AudioClip>();
    Dictionary<ESFX, AudioClip> m_sfxs = new Dictionary<ESFX, AudioClip>();

    //�÷����ϴ� AudioSource
    [SerializeField]
    AudioSource m_audioBgm;
    [SerializeField]
    AudioSource m_audioSfx;

    protected override void OnAwake()
    {
        Debug.Log($"BGM ������ ����: {m_BGMDatas.Count}");
        foreach (var data in m_BGMDatas)
        {
            m_bgms[data.key] = data.value;
        }
        Debug.Log($"��ϵ� BGM ����: {m_bgms.Count}");
        foreach (var data in m_SFXDatas)
        {
            m_sfxs[data.key] = data.value;
        }
    }

    public void PlayBGM(EBGM _bgm)
    {
        Debug.Log($"��ϵ� BGM ����: {m_bgms.Count}");
        if (m_bgms.ContainsKey(_bgm))
        {
            m_audioBgm.clip = m_bgms[_bgm];
            m_audioBgm.Play();
        }
    }

    public void StopBGM()
    {
        m_audioBgm.Stop();
    }

    public void PlaySFX(ESFX _sfx)
    {
        if (m_sfxs.ContainsKey(_sfx))
        {
            m_audioSfx.PlayOneShot(m_sfxs[_sfx]);
        }
    }

    public void SetVolume(float value)
    {
        m_audioBgm.volume = value;
        m_audioSfx.volume = value;
    }
}
