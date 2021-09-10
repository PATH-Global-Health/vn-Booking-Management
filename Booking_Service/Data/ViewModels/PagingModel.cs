using System;
using System.Collections.Generic;
using System.Text;

namespace Data.ViewModels
{
    public class PagingModel
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalPage { get; set; }
        public long TotalSize { get; set; }
        public object Data { get; set; }

        public PagingModel()
        {

        }

        public PagingModel(int pageIndex, int pageSize, long totalSize)
        {
            PageIndex = pageIndex <= 0 ? 1 : pageIndex;
            PageSize = pageSize <= 0 ? 5 : pageSize;
            TotalSize = totalSize;
            TotalPage = (int)Math.Ceiling(TotalSize / (double)PageSize);
        }
    }

}
