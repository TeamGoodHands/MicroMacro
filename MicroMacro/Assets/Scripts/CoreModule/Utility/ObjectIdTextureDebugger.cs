
using System.Text;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class ObjectIdTextureDebugger : MonoBehaviour
{
    [SerializeField]
    private bool dumpEveryFrame = false; // true にすると毎フレームダンプ

    [SerializeField, Tooltip("R16 系のフォーマットを使っている場合は true, R8 系なら false")]
    private bool useR16Format = true;

    [SerializeField, Tooltip("ログに出す最大ピクセル数")]
    private int maxLogCount = 2000;

    private void Update()
    {
        if (!dumpEveryFrame && !Input.GetKeyDown(KeyCode.F3))
        {
            return;
        }

        Texture globalTexture = Shader.GetGlobalTexture("_DebugObjectIdTexture");
        RenderTexture renderTexture = globalTexture as RenderTexture;

        if (renderTexture == null)
        {
            Debug.LogWarning("DebugObjectIdTexture is null or not a RenderTexture.");
            return;
        }

        AsyncGPUReadback.Request(renderTexture, 0, request =>
        {
            if (request.hasError)
            {
                Debug.LogError("AsyncGPUReadback error.");
                return;
            }

            int width = renderTexture.width;
            int height = renderTexture.height;

            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"ObjectIdTexture non zero pixels (width={width}, height={height})");

            int loggedCount = 0;

            if (useR16Format)
            {
                NativeArray<ushort> data = request.GetData<ushort>();

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int index = y * width + x;
                        ushort value = data[index];

                        if (value > 0)
                        {
                            builder.AppendLine($"({x}, {y}) = {value}");
                            loggedCount++;

                            if (loggedCount >= maxLogCount)
                            {
                                builder.AppendLine($"... truncated (over {maxLogCount} pixels)");
                                Debug.Log(builder.ToString());
                                return;
                            }
                        }
                    }
                }
            }
            else
            {
                NativeArray<byte> data = request.GetData<byte>();

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int index = y * width + x;
                        byte value = data[index];

                        if (value > 0)
                        {
                            builder.AppendLine($"({x}, {y}) = {value}");
                            loggedCount++;

                            if (loggedCount >= maxLogCount)
                            {
                                builder.AppendLine($"... truncated (over {maxLogCount} pixels)");
                                Debug.Log(builder.ToString());
                                return;
                            }
                        }
                    }
                }
            }

            if (loggedCount == 0)
            {
                builder.AppendLine("No non zero pixels found.");
            }

            Debug.Log(builder.ToString());
        });
    }
}
