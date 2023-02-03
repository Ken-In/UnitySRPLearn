using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public partial class CameraRenderer
{
    private ScriptableRenderContext context;//状态更新和绘制命令
    private Camera camera;
    private const string bufferName = "Render Camera";
    private CommandBuffer buffer = new CommandBuffer{name = bufferName};//命令缓冲区 可保存渲染命令列表
    
    public void Render(ScriptableRenderContext context, Camera camera, bool useDynamicBatching, bool useGPUInstancing)
    {
        this.context = context;
        this.camera = camera;

        PrepareBuffer();//profiler和frame debugger上的布局
        PrepareForSceneWindow();//显示ui
        if (!Cull())//剔除
            return;

        Setup();//渲染准备
        DrawVisibleGeometry(useDynamicBatching, useGPUInstancing);
        DrawUnsupportedShaders();
        DrawGizmos();
        Submit();//提交
    }
    
    void ExecuteBuffer()
    {
        context.ExecuteCommandBuffer(buffer);//注册命令到内部列表            
        buffer.Clear();//清空buffer
    }

    void Setup()
    {
        context.SetupCameraProperties(camera);
        CameraClearFlags flags = camera.clearFlags;
        buffer.ClearRenderTarget(
            flags <= CameraClearFlags.Depth, 
            flags == CameraClearFlags.Color,
            flags == CameraClearFlags.Color ?
                camera.backgroundColor.linear : Color.clear);
        buffer.BeginSample(SampleName);//开始记录
        ExecuteBuffer();
    }

    static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");
    
    void DrawVisibleGeometry(bool useDynamicBatching, bool useGPUInstancing)
    {
        var sortingSettings = new SortingSettings(camera) { criteria = SortingCriteria.CommonOpaque };//定义渲染分类
        var drawingSettings = new DrawingSettings(unlitShaderTagId, sortingSettings) //定义绘制选择
        {
            enableDynamicBatching = useDynamicBatching,
            enableInstancing = useGPUInstancing
        };
        GraphicsSettings.useScriptableRenderPipelineBatching = false;
        var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);//定义可见队列
        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
        
        context.DrawSkybox(camera);//绘制天空和
        
        //transparent配置
        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;
        filteringSettings.renderQueueRange = RenderQueueRange.transparent;
        
        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);//绘制transparent物体
    }

    //context 延迟 render 直到我们 submit 它
    void Submit()
    {
        buffer.EndSample(SampleName);//停止记录
        ExecuteBuffer();
        context.Submit();//执行命令
    }

    private CullingResults cullingResults;

    bool Cull()
    {
        if (camera.TryGetCullingParameters(out ScriptableCullingParameters p))
        {
            cullingResults = context.Cull(ref p);//从相机得到剔除参数
            return true;
        }
        return false;
    }

}
