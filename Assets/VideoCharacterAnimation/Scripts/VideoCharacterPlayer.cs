using UnityEngine;
using UnityEngine.Video;

[DisallowMultipleComponent]
[RequireComponent(typeof(VideoPlayer))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public sealed class VideoCharacterPlayer : MonoBehaviour
{
    private const string DefaultShaderName = "BB/Video/ChromaKeyMagenta";

    [Header("Video")]
    [SerializeField] private VideoClip videoClip;
    [SerializeField] private string resourcesPath = "CharAnim/Character";
    [SerializeField] private bool playOnEnable = true;
    [SerializeField] private bool loop = true;
    [SerializeField] private bool muteAudio = true;

    [Header("Render")]
    [SerializeField] private Material materialTemplate;
    [SerializeField] private int fallbackTextureWidth = 1024;
    [SerializeField] private int fallbackTextureHeight = 1024;
    [SerializeField] private Vector2 size = new Vector2(3f, 3f);
    [SerializeField] private bool fitToVideoAspect = true;
    [SerializeField] private string sortingLayerName = "Default";
    [SerializeField] private int sortingOrder;

    [Header("Chroma Key")]
    [SerializeField] private Color keyColor = Color.magenta;
    [SerializeField, Range(0.001f, 0.5f)] private float tolerance = 0.08f;
    [SerializeField, Range(0f, 0.5f)] private float feather = 0.035f;

    private VideoPlayer player;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private RenderTexture renderTexture;
    private Material runtimeMaterial;
    private Mesh runtimeMesh;

    public VideoPlayer Player
    {
        get
        {
            EnsureComponents();
            return player;
        }
    }

    public void Play()
    {
        EnsureReady();
        player.Play();
    }

    public void Pause()
    {
        if (player != null)
        {
            player.Pause();
        }
    }

    public void Stop()
    {
        if (player != null)
        {
            player.Stop();
        }
    }

    public void Replay()
    {
        EnsureReady();
        player.time = 0d;
        player.Play();
    }

    private void Awake()
    {
        EnsureReady();
    }

    private void OnEnable()
    {
        EnsureReady();

        if (playOnEnable && Application.isPlaying)
        {
            player.Play();
        }
    }

    private void OnDisable()
    {
        if (player != null)
        {
            player.Stop();
        }
    }

    private void OnDestroy()
    {
        ReleaseRuntimeObjects();
    }

    private void OnValidate()
    {
        fallbackTextureWidth = Mathf.Max(16, fallbackTextureWidth);
        fallbackTextureHeight = Mathf.Max(16, fallbackTextureHeight);
        size.x = Mathf.Max(0.01f, size.x);
        size.y = Mathf.Max(0.01f, size.y);

        if (isActiveAndEnabled)
        {
            EnsureReady();
            ApplyMaterialProperties();
            ApplyMesh();
        }
    }

    private void EnsureReady()
    {
        EnsureComponents();
        EnsureVideoClip();
        EnsureRenderTexture();
        EnsureMaterial();
        ApplyVideoSettings();
        ApplyRendererSettings();
        ApplyMesh();
    }

    private void EnsureComponents()
    {
        if (player == null)
        {
            player = GetComponent<VideoPlayer>();
            if (player == null)
            {
                player = gameObject.AddComponent<VideoPlayer>();
            }
        }

        if (meshFilter == null)
        {
            meshFilter = GetComponent<MeshFilter>();
            if (meshFilter == null)
            {
                meshFilter = gameObject.AddComponent<MeshFilter>();
            }
        }

        if (meshRenderer == null)
        {
            meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer == null)
            {
                meshRenderer = gameObject.AddComponent<MeshRenderer>();
            }
        }
    }

    private void EnsureVideoClip()
    {
        if (videoClip == null && !string.IsNullOrWhiteSpace(resourcesPath))
        {
            videoClip = Resources.Load<VideoClip>(resourcesPath);
        }
    }

    private void EnsureRenderTexture()
    {
        int width = fallbackTextureWidth;
        int height = fallbackTextureHeight;

        if (videoClip != null && videoClip.width > 0 && videoClip.height > 0)
        {
            width = (int)videoClip.width;
            height = (int)videoClip.height;
        }

        if (renderTexture != null && renderTexture.width == width && renderTexture.height == height)
        {
            return;
        }

        if (renderTexture != null)
        {
            renderTexture.Release();
            DestroyUnityObject(renderTexture);
        }

        renderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32)
        {
            name = $"{name}_VideoRT",
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
            useMipMap = false,
            autoGenerateMips = false
        };
        renderTexture.Create();
    }

    private void EnsureMaterial()
    {
        if (runtimeMaterial == null)
        {
            Material source = materialTemplate;
            if (source == null)
            {
                Shader shader = Shader.Find(DefaultShaderName);
                if (shader != null)
                {
                    source = new Material(shader);
                }
            }

            if (source != null)
            {
                runtimeMaterial = Instantiate(source);
                runtimeMaterial.name = $"{name}_VideoMaterial";
                runtimeMaterial.hideFlags = HideFlags.DontSave;
            }
        }

        if (runtimeMaterial != null)
        {
            meshRenderer.sharedMaterial = runtimeMaterial;
            ApplyMaterialProperties();
        }
    }

    private void ApplyVideoSettings()
    {
        player.playOnAwake = false;
        player.isLooping = loop;
        player.source = VideoSource.VideoClip;
        player.clip = videoClip;
        player.renderMode = VideoRenderMode.RenderTexture;
        player.targetTexture = renderTexture;
        player.waitForFirstFrame = true;
        player.skipOnDrop = true;

        if (muteAudio)
        {
            player.audioOutputMode = VideoAudioOutputMode.None;
        }
    }

    private void ApplyRendererSettings()
    {
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;
        meshRenderer.allowOcclusionWhenDynamic = false;
        meshRenderer.sortingLayerName = sortingLayerName;
        meshRenderer.sortingOrder = sortingOrder;
    }

    private void ApplyMaterialProperties()
    {
        if (runtimeMaterial == null)
        {
            return;
        }

        runtimeMaterial.mainTexture = renderTexture;
        runtimeMaterial.SetTexture("_MainTex", renderTexture);
        runtimeMaterial.SetColor("_KeyColor", keyColor);
        runtimeMaterial.SetFloat("_Tolerance", tolerance);
        runtimeMaterial.SetFloat("_Feather", feather);
    }

    private void ApplyMesh()
    {
        if (runtimeMesh == null)
        {
            runtimeMesh = new Mesh
            {
                name = $"{name}_VideoQuad"
            };
        }

        float width = size.x;
        float height = size.y;

        if (fitToVideoAspect && videoClip != null && videoClip.width > 0 && videoClip.height > 0)
        {
            width = size.y * ((float)videoClip.width / videoClip.height);
        }

        float halfWidth = width * 0.5f;
        float halfHeight = height * 0.5f;

        runtimeMesh.Clear();
        runtimeMesh.vertices = new[]
        {
            new Vector3(-halfWidth, -halfHeight, 0f),
            new Vector3(halfWidth, -halfHeight, 0f),
            new Vector3(-halfWidth, halfHeight, 0f),
            new Vector3(halfWidth, halfHeight, 0f)
        };
        runtimeMesh.uv = new[]
        {
            new Vector2(0f, 0f),
            new Vector2(1f, 0f),
            new Vector2(0f, 1f),
            new Vector2(1f, 1f)
        };
        runtimeMesh.triangles = new[] { 0, 2, 1, 2, 3, 1 };
        runtimeMesh.RecalculateBounds();

        meshFilter.sharedMesh = runtimeMesh;
    }

    private void ReleaseRuntimeObjects()
    {
        if (renderTexture != null)
        {
            renderTexture.Release();
            DestroyUnityObject(renderTexture);
            renderTexture = null;
        }

        if (runtimeMaterial != null)
        {
            DestroyUnityObject(runtimeMaterial);
        }

        runtimeMaterial = null;

        if (runtimeMesh != null)
        {
            DestroyUnityObject(runtimeMesh);
            runtimeMesh = null;
        }
    }

    private static void DestroyUnityObject(Object target)
    {
        if (target == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(target);
        }
        else
        {
            DestroyImmediate(target);
        }
    }
}
