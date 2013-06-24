
using UnityEngine;
using System;
using System.Collections;

/*
    アルゴリズムの考え方は  http://www5f.biglobe.ne.jp/~kenmo/program/dangeon2/dangeon2.html
*/

namespace Dungeion
{
    /// <summary>
    /// クラス：ダンジョン
    /// </summary>
    public class DungeionMap
    {
        public const int     AreaRowNum  = 6;
        public const int     AreaLineNum = 6;

        public MapArea[,]    AreaList;


        /// <summary>
        /// ダンジョン生成
        /// </summary>
        public void Generate()
        {
            this.AreaList = new MapArea[AreaLineNum,AreaRowNum];

            // エリアの種類を決定
            for(int y = 0; y < this.AreaList.GetLength(0); y++){
                for(int x = 0; x < this.AreaList.GetLength(1); x++){
                    this.CalcAreaType(x,y);
                }
            }

            // エリア間のつながりを決定
            for(int y = 0; y < this.AreaList.GetLength(0); y++){
                for(int x = 0; x < this.AreaList.GetLength(1); x++){
                    this.CalcAreaConnection(x,y);
                }
            }
            m_nowBlockID = 0;
            for(int y = 0; y < this.AreaList.GetLength(0); y++){
                for(int x = 0; x < this.AreaList.GetLength(1); x++){
                    this.CalcAreaBlockID(x,y);
                }
            }
            // TODO : ブロックをつなげる

            Debug.Log("[DungeionMap] Generated!!");
        }
        private void CalcAreaType(int x,int y)
        {
            int typeNum = Enum.GetNames(typeof(MapAreaType)).Length;

            var info = new MapArea();
            info.Type = (MapAreaType)m_random.Next(typeNum); // TODO : 各種類ごとに確率いるかもね。
            this.AreaList[y,x] = info;
        }

        private void CalcAreaConnection(int x,int y)
        {
            MapArea info = this.AreaList[y,x];
            if( info.Type == MapAreaType.None ){
                return;
            }

            // Upper
            MapArea connectArea;
            if( m_random.Next(3) < 1
                && y > 0 ){
                connectArea = this.AreaList[y - 1,x];
                if( info.ConnectDir[(int)MapAreaTypeDir.Upper] == null
                    && connectArea.Type != MapAreaType.None ){
                    info.ConnectDir[(int)MapAreaTypeDir.Upper]         = connectArea;
                    connectArea.ConnectDir[(int)MapAreaTypeDir.Bottom] = info;
                }
            }
            // Bottom
            if( m_random.Next(3) < 1
                && y < this.AreaList.GetLength(0) - 1 ){
                connectArea = this.AreaList[y + 1,x];
                if( info.ConnectDir[(int)MapAreaTypeDir.Bottom] == null
                    && connectArea.Type != MapAreaType.None ){
                    info.ConnectDir[(int)MapAreaTypeDir.Bottom]       = connectArea;
                    connectArea.ConnectDir[(int)MapAreaTypeDir.Upper] = info;
                }
            }
            // Left
            if( m_random.Next(3) < 1
                && x > 0 ){
                connectArea = this.AreaList[y,x - 1];
                if( info.ConnectDir[(int)MapAreaTypeDir.Left] == null
                    && connectArea.Type != MapAreaType.None ){
                    info.ConnectDir[(int)MapAreaTypeDir.Left]         = connectArea;
                    connectArea.ConnectDir[(int)MapAreaTypeDir.Right] = info;
                }
            }
            // Right
            if( m_random.Next(3) < 1
                && x < this.AreaList.GetLength(1) - 1 ){
                connectArea = this.AreaList[y,x + 1];
                if( info.ConnectDir[(int)MapAreaTypeDir.Right] == null
                    && connectArea.Type != MapAreaType.None ){
                    info.ConnectDir[(int)MapAreaTypeDir.Right]       = connectArea;
                    connectArea.ConnectDir[(int)MapAreaTypeDir.Left] = info;
                }
            }
        }

        /// 到達できるエリア群ごとに ID をつけてく.
        private void CalcAreaBlockID(int x,int y)
        {
            MapArea info = this.AreaList[y,x];
            if( info.Type != MapAreaType.Room
                || info.BlockID >= 0 ){  // すでにブロック判定済み
                return;
            }

            this.CalcAreaBlockID(info);
            ++m_nowBlockID;
        }
        private void CalcAreaBlockID(MapArea info)
        {
            if( info.BlockID >= 0 ){  // すでにブロック判定済み
                return;
            }
            info.BlockID = m_nowBlockID;

            MapArea checkArea;
            for(int i = 0; i < info.ConnectDir.Length; i++){
                checkArea = info.ConnectDir[i];
                if( checkArea != null ){
                    this.CalcAreaBlockID(checkArea);
                }
            }
        }


        private int m_nowBlockID = 0;
        private System.Random   m_random = new System.Random();
    }



    /// エリア種類
    public enum MapAreaType
    {
        None,     ///< なし
        Room,     ///< 部屋がある
        Road,     ///< 通路だけ
    }
    public enum MapAreaTypeDir
    {
        Upper,
        Bottom,
        Left,
        Right
    }
    public class MapArea
    {
        public MapAreaType  Type;
        public MapArea[]    ConnectDir = new MapArea[4];
        public int          BlockID = -1;
    }
}
