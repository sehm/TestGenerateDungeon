
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
        public const int     AreaLineNum = 4;

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
            var info = new MapArea();
            info.X = x;
            info.Y = y;

            // まず部屋を確定.
            info.Type = m_random.Next(6) > 1 ? MapAreaType.None : MapAreaType.Room;
            this.AreaList[y,x] = info;
        }


        #region エリアごとをつなぐ処理：一回目
        /// エリアごとをつなぐ処理：一回目
        private void CalcAreaConnection(int x,int y)
        {
            MapArea info = this.AreaList[y,x];
            if( info.Type == MapAreaType.None ){
                return;
            }

            // 上下
            if( m_random.Next(3) < 1
                && y > 0
                && this.AreaList[y,x].ConnectDir[(int)MapAreaTypeDir.Upper] == null ){
                this.StraightRoadToHorizonal(info, MapAreaTypeDir.Upper);
            }
            if( m_random.Next(3) < 1
                && y < this.AreaList.GetLength(0) - 1
                && this.AreaList[y,x].ConnectDir[(int)MapAreaTypeDir.Bottom] == null ){
                this.StraightRoadToHorizonal(info, MapAreaTypeDir.Bottom);
            }
            // 左右
            if( m_random.Next(3) < 1
                && x > 0
                && this.AreaList[y,x].ConnectDir[(int)MapAreaTypeDir.Left] == null ){
                this.StraightRoadToHorizonal(info, MapAreaTypeDir.Left);
            }
            if( m_random.Next(3) < 1
                && x < this.AreaList.GetLength(1) - 1
                && this.AreaList[y,x].ConnectDir[(int)MapAreaTypeDir.Right] == null ){
                this.StraightRoadToHorizonal(info, MapAreaTypeDir.Right);
            }
        }

        /// 指定方向にエリアを検索し、部屋を見つけたらつなげる
        private bool StraightRoadToHorizonal(MapArea fromArea, MapAreaTypeDir dir)
        {
            // 進行方向のサイドのエリアをチェック
            if( this.CheckStraightSideArea(fromArea, dir) ){
                return true;
            }

            // その先を検索
            MapArea connectArea = GetNextMapArea(fromArea.X,fromArea.Y, dir);
            if( connectArea == null ){
                return false;
            }
            if( connectArea.Type == MapAreaType.None ){
                // なにもない
                if( this.StraightRoadToHorizonal(connectArea, dir) ){
                    ConnectArea(fromArea,connectArea, dir);
                    return true;
                }else{
                    return false;
                }
            }else{// if( connectArea.Type == MapAreaType.Room || connectArea.Type == MapAreaType.Road ){
                // 部屋につながった
                ConnectArea(fromArea,connectArea, dir);
                return true;
            }
        }

        private bool CheckStraightSideArea(MapArea fromArea,MapAreaTypeDir dir)
        {
            if( fromArea.Type != MapAreaType.None ){
                return false;
            }

            // つながるとこがあるならつなげておしまい。
            if( dir == MapAreaTypeDir.Upper || dir == MapAreaTypeDir.Bottom ){
                // 進行方向が上下
                if( CheckStraightSideAreaInternal(fromArea, MapAreaTypeDir.Left) ){
                    return true;
                }else if( CheckStraightSideAreaInternal(fromArea, MapAreaTypeDir.Right) ){
                    return true;
                }
            }else{//if( dir == MapAreaTypeDir.Left || dir == MapAreaTypeDir.Right ){
                // 進行方向が左右
                if( CheckStraightSideAreaInternal(fromArea, MapAreaTypeDir.Upper) ){
                    return true;
                }else if( CheckStraightSideAreaInternal(fromArea, MapAreaTypeDir.Bottom) ){
                    return true;
                }
            }
            return false;
        }
        private bool CheckStraightSideAreaInternal(MapArea fromArea,MapAreaTypeDir dir)
        {
            MapArea connectArea = this.GetNextMapArea(fromArea.X,fromArea.Y, dir);
            if( connectArea != null
                //&& connectArea.Type != MapAreaType.None ){
                && connectArea.Type == MapAreaType.Room ){
                ConnectArea(fromArea,connectArea, dir);
                return true;
            }
            return false;
        }

        private MapArea GetNextMapArea(int x,int y, MapAreaTypeDir dir)
        {
            // 上下方向
            int next = 0;
            if( dir == MapAreaTypeDir.Upper ){
                next = y - 1;
                if( next < 0 ){
                    return null;// マップの端っこに到達
                }
                return this.AreaList[next,x];
            }else if( dir == MapAreaTypeDir.Bottom ){
                next = y + 1;
                if( this.AreaList.GetLength(0) <= next ){
                    return null;// マップの端っこに到達
                }
                return this.AreaList[next,x];
            }

            // 左右方向
            if( dir == MapAreaTypeDir.Left ){
                next = x - 1;
                if( next < 0 ){
                    return null;// マップの端っこに到達
                }
                return this.AreaList[y,next];
            }else if( dir == MapAreaTypeDir.Right ){
                next = x + 1;
                if( this.AreaList.GetLength(1) <= next ){
                    return null;// マップの端っこに到達
                }
                return this.AreaList[y,next];
            }
            return null;
        }

        private static void ConnectArea(MapArea from,MapArea to, MapAreaTypeDir dir)
        {
            from.ConnectDir[(int)dir] = to;
            if( from.Type == MapAreaType.None ){
                from.Type = MapAreaType.Road;
            }
        }
        #endregion


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
        public int X;
        public int Y;

        public MapAreaType  Type;
        public MapArea[]    ConnectDir = new MapArea[4];
        public int          BlockID = -1;
    }
}
