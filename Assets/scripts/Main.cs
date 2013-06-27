
#define _NO_DISP_ROAD_AREA_CUBE_   ///< 道エリアのキューブを表示しない.

using UnityEngine;
using System;
using System.Collections;
using Dungeion;

public class Main : MonoBehaviour
{

    void Start ()
    {
        m_map = new DungeionMap();
        m_map.Generate();

        this.InitCamera();
        this.InitGridDisp();
    }

    /// 初期化：グリッド表示
    private void InitGridDisp()
    {
        m_dispGridList = new GameObject[DungeionMap.AreaLineNum,DungeionMap.AreaRowNum];

        m_gridRoot = new GameObject("Grid");
        m_gridRoot.transform.position = Vector3.zero;

        for(int y = 0; y < m_map.AreaList.GetLength(0); y++){
            for(int x = 0; x < m_map.AreaList.GetLength(1); x++){
                m_dispGridList[y,x] = this.CreateCube(x,y);
            }
        }
        for(int y = 0; y < m_map.AreaList.GetLength(0); y++){
            for(int x = 0; x < m_map.AreaList.GetLength(1); x++){
                this.InitDispArea(x,y);
            }
        }
    }
    private GameObject CreateCube(int x,int y)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);

        var trans = go.transform;
        trans.parent = m_gridRoot.transform;
        trans.localPosition = new Vector3(0f + GridWidth * x, 0f - GridHeight * y, 0f);
        trans.localScale    = new Vector3(10f,10f,10f);
        return go;
    }

    /// 初期化：エリア表示
    private void InitDispArea(int x,int y)
    {
        var info = m_map.AreaList[y,x];
        var go   = m_dispGridList[y,x];

        Color col = Color.white;
        switch(info.Type){
            case MapAreaType.None: col = Color.gray;  break;
            case MapAreaType.Room: col = Color.green; break;
            case MapAreaType.Road: col = Color.gray;  break;
        }
        go.renderer.material.color = col;

#if _NO_DISP_ROAD_AREA_CUBE_
        if( info.Type == MapAreaType.Road ){
            go.renderer.enabled = false;
        }
#endif

        // つながっているエリア同士のとこに線を引く
        if( info.ConnectDir[(int)MapAreaTypeDir.Upper] != null ){
            this.DrawLine(go.transform.position, m_dispGridList[y - 1,x].transform.position);
        }
        if( info.ConnectDir[(int)MapAreaTypeDir.Bottom] != null ){
            this.DrawLine(go.transform.position, m_dispGridList[y + 1,x].transform.position);
        }
        if( info.ConnectDir[(int)MapAreaTypeDir.Left] != null ){
            this.DrawLine(go.transform.position, m_dispGridList[y,x - 1].transform.position);
        }
        if( info.ConnectDir[(int)MapAreaTypeDir.Right] != null ){
            this.DrawLine(go.transform.position, m_dispGridList[y,x + 1].transform.position);
        }
    }
    private GameObject DrawLine(Vector3 startPos,Vector3 endPos)
    {
        var go = new GameObject("Line");

        var trans = go.transform;
        trans.parent = m_gridRoot.transform;
        trans.position = Vector3.zero;

        var lineRenderer = go.AddComponent<LineRenderer>();
        lineRenderer.material = new Material (Shader.Find("Transparent/VertexLit"));
        lineRenderer.SetWidth(2,2);

        lineRenderer.SetVertexCount(2);
        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(1, endPos);

        return go;
    }


    /// カメラ設定
    private void InitCamera()
    {
        var cam = Camera.mainCamera;

        cam.transform.position = new Vector3(GridWidth * DungeionMap.AreaRowNum / 2,
                                             -1f * (GridHeight * DungeionMap.AreaLineNum / 2f),
                                             -40f);

        cam.orthographicSize = Mathf.Max(GridWidth * DungeionMap.AreaRowNum, GridHeight * DungeionMap.AreaLineNum);
    }


    private const float GridWidth  = 20f;
    private const float GridHeight = 20f;

    private DungeionMap      m_map;
    private GameObject[,]    m_dispGridList;

    private GameObject       m_gridRoot;
}
