
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

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
        public int     AreaRowNum  { get; private set; }
        public int     AreaLineNum { get; private set; }

        public MapArea[,]    AreaList;


        /// <summary>
        /// グリッドの幅と高さを指定してダンジョン生成
        /// </summary>
        public void Generate(int rowLength,int lineLength)
        {
            this.AreaRowNum  = rowLength;
            this.AreaLineNum = lineLength;
            this.AreaList = new MapArea[lineLength,rowLength];

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

            // ブロックを判定、 つながっていないブロックをつなげる
            this.CalcBlockConnection();

            // TODO : 階段の位置を確定
            // アルゴリズムとしての候補は
            //      ・部屋エリアについて、ランダムで開始エリア、終了エリアを決定
            //      ・部屋エリアについて、ランダムで開始エリアを決定.  そこから一番遠い部屋を終了エリアとする。遠いかの判断はグリッドの直線距離でいいんでないかな.

            Debug.Log("[DungeionMap] Generated!!");
        }


        /// 指定グリッドのエリアの種類を確定
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
            if( info.Type != MapAreaType.Room ){
                return;
            }

            int connectNum = 0;
            // 上下
            if( m_random.Next(3) < 1
                && y > 0
                && info.ConnectDir[(int)MapAreaTypeDir.Upper] == null ){
                if( this.CheckStraightRoad(info, MapAreaTypeDir.Upper) ){
                    ++connectNum;
                }
            }
            if( m_random.Next(3) < 1
                && y < this.AreaList.GetLength(0) - 1
                && info.ConnectDir[(int)MapAreaTypeDir.Bottom] == null ){
                if( this.CheckStraightRoad(info, MapAreaTypeDir.Bottom) ){
                    ++connectNum;
                }
            }
            // 左右
            if( m_random.Next(3) < 1
                && x > 0
                && info.ConnectDir[(int)MapAreaTypeDir.Left] == null ){
                if( this.CheckStraightRoad(info, MapAreaTypeDir.Left) ){
                    ++connectNum;
                }
            }
            if( m_random.Next(3) < 1
                && x < this.AreaList.GetLength(1) - 1
                && info.ConnectDir[(int)MapAreaTypeDir.Right] == null ){
                if( this.CheckStraightRoad(info, MapAreaTypeDir.Right) ){
                    ++connectNum;
                }
            }

            if( connectNum <= 0 ){
                // 必ず一つはつなげる
                Debug.Log("no connet (" + x.ToString() + ", " + y.ToString() + ")");
                foreach(MapAreaTypeDir dir in Enum.GetValues(typeof(MapAreaTypeDir))){
                    if( info.ConnectDir[(int)dir] != null ){
                        return;
                    }
                }

                foreach(MapAreaTypeDir dir in Enum.GetValues(typeof(MapAreaTypeDir))){
                    if( info.ConnectDir[(int)dir] == null
                        && this.CheckStraightRoad(info, dir) ){
                        return;
                    }
                }
            }
        }

        /// 指定方向にまっすぐエリアを検索し、部屋を見つけたらつなげる
        private bool CheckStraightRoad(MapArea fromArea, MapAreaTypeDir dir)
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
                if( this.CheckStraightRoad(connectArea, dir) ){
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
            to.ConnectDir[ (int)MapAreaReverseDirList[(int)dir] ] = from;

            if( from.Type == MapAreaType.None ){
                from.Type = MapAreaType.Road;
            }
        }

        #endregion

        #region ブロックをつなげる

        /// ブロックがひとつになるまでつなげる
        private void CalcBlockConnection()
        {
            // ひとつとなりで連結仕様としてもダメだったら、直線で連結をためす
            m_blockConnectType = BlockConnectType.One;

            this.MarkAreaBlockID();
            while(m_nowBlockID > 1){
                if( m_blockConnectType == BlockConnectType.One ){
                    if( !this.CalcBlockConnectOne() ){
                        m_blockConnectType = BlockConnectType.Line;
                        continue;
                    }
                }else{// if( m_blockConnectType == BlockConnectType.Line ){
                    if( !CalcBlockConnectLine() ){
                        Debug.Log("[DungeionMap] Fatal Error!!");
                        return;
                    }
                }

                this.MarkAreaBlockID();
            }
        }

        /// 到達できるエリア群ごとに ID をつけてく.
        private void MarkAreaBlockID()
        {
            for(int y = 0; y < this.AreaList.GetLength(0); y++){
                for(int x = 0; x < this.AreaList.GetLength(1); x++){
                    this.AreaList[y,x].BlockID = MapArea.BlockIDNone;
                }
            }
            m_nowBlockID = 0;

            for(int y = 0; y < this.AreaList.GetLength(0); y++){
                for(int x = 0; x < this.AreaList.GetLength(1); x++){
                    this.CalcAreaBlockID(x,y);
                }
            }
            Debug.Log("MarkAreaBlockID() : now block num = " + m_nowBlockID.ToString());
        }
        private void CalcAreaBlockID(int x,int y)
        {
            MapArea info = this.AreaList[y,x];
            if( info.Type == MapAreaType.None
                || info.BlockID >= 0 ){  // すでにブロック判定済み
                return;
            }

            this.CalcAreaBlockID(info);
            ++m_nowBlockID;
        }
        private void CalcAreaBlockID(MapArea info)
        {
            if( info.Type == MapAreaType.None
                || info.BlockID >= 0 ){  // すでにブロック判定済み
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

        /// 部屋エリアについて、隣同士が別ブロックだったら連結する。  １回だけ
        private bool CalcBlockConnectOne()
        {
            MapArea info;
            for(int y = 0; y < this.AreaList.GetLength(0) - 1; y++){
                for(int x = 0; x < this.AreaList.GetLength(1) - 1; x++){
                    info = this.AreaList[y,x];
                    if( info.Type == MapAreaType.None ){
                        continue;
                    }

                    // 右と下のエリアをチェック
                    if( this.CalcBlockConnectOne(info, x + 1, y, MapAreaTypeDir.Right)
                        || this.CalcBlockConnectOne(info, x, y + 1, MapAreaTypeDir.Bottom) ){
                        return true;
                    }
                }
            }
            return false;// つなげることができなかった.
        }
        private bool CalcBlockConnectOne(MapArea from, int x,int y, MapAreaTypeDir dir)
        {
            MapArea checkArea = this.AreaList[y,x];
            if( checkArea.Type != MapAreaType.None
                && checkArea.BlockID != from.BlockID ){
                // 違うブロックが隣あっているので、連結する。
                Debug.Log("Connect Block : " + from.BlockID.ToString() + ", " + checkArea.BlockID.ToString());
                ConnectArea(from, checkArea, dir);
                return true;
            }
            return false;
        }

        /// 部屋エリアについて、直線で道を伸ばした先が別ブロックだったら連結する。  １回だけ
        private bool CalcBlockConnectLine()
        {
            MapArea info;
            for(int y = 0; y < this.AreaList.GetLength(0) - 1; y++){
                for(int x = 0; x < this.AreaList.GetLength(1) - 1; x++){
                    info = this.AreaList[y,x];
                    if( info.Type == MapAreaType.None ){
                        continue;
                    }

                    // 右と下のエリアをチェック
                    if( this.CalcBlockConnectLine(info, x + 1, y, MapAreaTypeDir.Right)
                        || this.CalcBlockConnectLine(info, x, y + 1, MapAreaTypeDir.Bottom) ){
                        return true;
                    }
                }
            }
            return false;// つなげることができなかった.
        }
        private bool CalcBlockConnectLine(MapArea from, int x,int y, MapAreaTypeDir dir)
        {
            MapArea checkArea = this.AreaList[y,x];
            if( checkArea.Type != MapAreaType.None ){
                if( checkArea.BlockID == from.BlockID ){
                    return false;// 同一ブロックにつながった
                }else{
                    // 違うブロックが隣あっているので、連結する。
                    Debug.Log("Connect Block Line : " + string.Format("({0},{1})",from.X,from.Y) + " <=> " + string.Format("({0},{1})",checkArea.X,checkArea.Y));
                    ConnectArea(from, checkArea, dir);
                    return true;
                }
            }else{
                // なにもないエリア：次のエリアへ
                int nextX = x + (dir == MapAreaTypeDir.Right ? 1 : 0);
                if( nextX >= this.AreaList.GetLength(1) ){
                    return false;
                }
                int nextY = y + (dir == MapAreaTypeDir.Bottom ? 1 : 0);
                if( nextY >= this.AreaList.GetLength(0) ){
                    return false;
                }

                if( CalcBlockConnectLine(checkArea, nextX,nextY, dir) ){
                    ConnectArea(from, checkArea, dir);
                    return true;
                }
            }
            return false;
        }
        #endregion


        private static readonly MapAreaTypeDir[] MapAreaReverseDirList = {
            MapAreaTypeDir.Bottom,
            MapAreaTypeDir.Upper,
            MapAreaTypeDir.Right,
            MapAreaTypeDir.Left,
        };

        private enum BlockConnectType
        {
            One,
            Line,
        }
        private BlockConnectType   m_blockConnectType;
        private int                m_nowBlockID = 0;

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
        public MapArea[]    ConnectDir  = new MapArea[4];
        public int          BlockID     = BlockIDNone;
        public const int    BlockIDNone = -1;
    }
}
