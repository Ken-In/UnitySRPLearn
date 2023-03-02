using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// pipeline 会调用这个渲染器的render函数
public partial class CameraRenderer
{
    private const string bufferName = "Render Camera";//PrepareBuffer方法会重设 bufferName
    
    ScriptableRenderContext context;

    Camera camera;

    // 一些特定的渲染可以用专用方法调用（如天空盒），但其他命令需要缓冲区间接发出
    // 创建一个新的缓冲区实例
    private CommandBuffer buffer = new CommandBuffer
    {
        name = bufferName // 给名字以便在 frame debugger识别
    };

    private CullingResults cullingResults;
    
    static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");
    

    public void Render (ScriptableRenderContext context, Camera camera,
        bool useDynamicBatching, bool useGPUInstancing) {
        this.context = context;
        this.camera = camera;

        PrepareBuffer();
        PrepareForSceneWindow();
        if (!Cull())
            return;
        
        // 设置相机属性和 buffer
        Setup();
        
        // 渲染命令-------------------------------------------------
        // -------------------------------------------------------
        DrawVisibleGeometry(useDynamicBatching, useGPUInstancing);
        DrawUnsupportedShaders();
        DrawGizmos();
        
        // 提交命令后 执行命令
        Submit();
    }

    // 可视物体渲染命令
    void DrawVisibleGeometry(bool useDynamicBatching, bool useGPUInstancing)
    {   //opaque obj render----------------------------------------
        //---------------------------------------------------------
        var sortingSettings = new SortingSettings(camera) // 对象排序的方法
        {
            criteria = SortingCriteria.CommonOpaque // 按照opaque排序 从近到远渲染
        };
        var drawingSettings = new DrawingSettings( // 描述如何对可见对象进行排序 使用哪些着色器通道
            unlitShaderTagId, sortingSettings
        )
        {
            enableDynamicBatching = useDynamicBatching,
            enableInstancing = useGPUInstancing
        };
        var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);// 过滤渲染对象
        
        // 上下文 提交绘制物体命令
        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
        
        //sky box--------------------------------------------------
        //---------------------------------------------------------
        context.DrawSkybox(camera);
        
        //transparent obj render-----------------------------------
        //---------------------------------------------------------
        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;
        filteringSettings.renderQueueRange = RenderQueueRange.transparent;
        
        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
    }

    void Setup () {
        context.SetupCameraProperties(camera);
        CameraClearFlags flags = camera.clearFlags;
        buffer.ClearRenderTarget(
            flags <= CameraClearFlags.Depth, // 所有情况都需要清除depth 除了nothing
            flags == CameraClearFlags.Color, // 相等则清除color
            flags == CameraClearFlags.Color ? camera.backgroundColor.linear : Color.clear);// 清空RT 避免之前的渲染影响当前帧
        buffer.BeginSample(SampleName); // buffer开始采样 之后的渲染命令都会在buffer下
        ExecuteBuffer();// 执行清空buffer
    }
    
    void Submit () {
        buffer.EndSample(SampleName);// buffer结束采样
        ExecuteBuffer();// 执行buffer
        context.Submit();
    }
    
    void ExecuteBuffer () {
        context.ExecuteCommandBuffer(buffer); // 将buffer中的命令加入list 在submit中会真正执行
        buffer.Clear();
    }

    bool Cull () // 成功则记录 cullingResults
    {
        // ScriptableCullingParameters 用于配置可编程渲染管线中的剔除操作的参数
        if (camera.TryGetCullingParameters(out ScriptableCullingParameters p))
        {
            cullingResults = context.Cull(ref p);//剔除参数传入上下文 返回剔除结果
            return true;
        }
        return false;
    }
}
