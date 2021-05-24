using Data.DataAccess;
using Data.MongoCollections;
using Data.ViewModels;
using MongoDB.Driver;
using MoreLinq.Extensions;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using Services.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Core
{
    public interface IExcelService
    {
        Task<ResultModel> ExamReport(Guid unitId, DateTime dateTaken);
        Task<ResultModel> AvailableDatesForExamReport(Guid unitId);
        Task<ResultModel> VaccReport(Guid unitId, DateTime fromDate, DateTime toDate);
        Task<ResultModel> AvailableDatesForVaccReport(Guid unitId);
    }
    public class ExcelService : IExcelService
    {
        private readonly ApplicationDbContext _dbContext;

        public ExcelService(ApplicationDbContext context)
        {
            _dbContext = context;
        }

        public async Task<ResultModel> AvailableDatesForExamReport(Guid unitId)
        {
            var result = new ResultModel();
            try
            {
                var basefilter = Builders<Examination>.Filter.Empty;
                var hospitalIdFilter = Builders<Examination>.Filter.Eq(mt => mt.Unit.Id, unitId);
                basefilter = basefilter & hospitalIdFilter;

                var query = await _dbContext.Examinations.Find(basefilter).Project(e => e.Date).ToListAsync();
                var distincted = query.Select(q => q.Date).Distinct();
                result.Data = distincted;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.Succeed = false;
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public async Task<ResultModel> AvailableDatesForVaccReport(Guid unitId)
        {
            var result = new ResultModel();
            try
            {
                var basefilter = Builders<Vaccination>.Filter.Empty;
                var hospitalIdFilter = Builders<Vaccination>.Filter.Eq(mt => mt.Unit.Id, unitId);
                basefilter = basefilter & hospitalIdFilter;

                var query = await _dbContext.Vaccinations.Find(basefilter).Project(e => e.Date).ToListAsync();
                var distincted = query.Select(q => q.Date).Distinct();
                result.Data = distincted;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.Succeed = false;
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public async Task<ResultModel> ExamReport(Guid unitId, DateTime dateTaken)
        {
            var result = new ResultModel();
            try
            {
                var basefilter = Builders<Examination>.Filter.Empty;
                var hospitalIdFilter = Builders<Examination>.Filter.Eq(mt => mt.Unit.Id, unitId);
                basefilter = basefilter & hospitalIdFilter;
                var dateFilter = Builders<Examination>.Filter.Where(mt => mt.Date == dateTaken.Date);
                basefilter = basefilter & dateFilter;
                var query = await _dbContext.Examinations.FindAsync(basefilter);
                var rs = await query.ToListAsync();
                if (rs.Count == 0)
                {
                    throw new Exception("Không có mẫu.");
                }
                var locationAddress = rs.FirstOrDefault().Unit.Address;
                using (var ms = new MemoryStream())
                {

                    IWorkbook workbook = new XSSFWorkbook();
                    ISheet excelSheet = workbook.CreateSheet("Danh sach xn");

                    #region Styling
                    // Fonts
                    // boldFont
                    IFont boldFont = workbook.CreateFont();
                    boldFont.IsBold = true;
                    boldFont.FontHeightInPoints = 13;
                    // table header font
                    IFont tableHeaderFont = workbook.CreateFont();
                    tableHeaderFont.IsBold = true;
                    tableHeaderFont.FontHeightInPoints = 11;
                    // CellStyles
                    // normal left align cell
                    ICellStyle leftCellStyle = workbook.CreateCellStyle();
                    leftCellStyle.Alignment = HorizontalAlignment.Left;
                    // normal center cell
                    ICellStyle centerCellStyle = workbook.CreateCellStyle();
                    centerCellStyle.Alignment = HorizontalAlignment.Center;
                    // bold center cell
                    ICellStyle centerBoldCellStyle = workbook.CreateCellStyle();
                    centerBoldCellStyle.Alignment = HorizontalAlignment.Center;
                    centerBoldCellStyle.SetFont(boldFont);
                    // table header cell style
                    ICellStyle tableHeaderStyle = workbook.CreateCellStyle();
                    tableHeaderStyle.Alignment = HorizontalAlignment.Center;
                    tableHeaderStyle.VerticalAlignment = VerticalAlignment.Center;
                    tableHeaderStyle.SetFont(tableHeaderFont);
                    tableHeaderStyle.BorderTop = BorderStyle.Thin;
                    tableHeaderStyle.BorderBottom = BorderStyle.Thin;
                    tableHeaderStyle.BorderRight = BorderStyle.Thin;
                    tableHeaderStyle.BorderLeft = BorderStyle.Thin;
                    tableHeaderStyle.WrapText = true;
                    // table data style
                    ICellStyle tableDataStyle = workbook.CreateCellStyle();
                    tableDataStyle.Alignment = HorizontalAlignment.Center;
                    tableDataStyle.VerticalAlignment = VerticalAlignment.Center;
                    tableDataStyle.BorderTop = BorderStyle.Thin;
                    tableDataStyle.BorderBottom = BorderStyle.Thin;
                    tableDataStyle.BorderRight = BorderStyle.Thin;
                    tableDataStyle.BorderLeft = BorderStyle.Thin;
                    tableDataStyle.WrapText = true;
                    #endregion

                    #region Header
                    int rowIndex = 0;
                    IRow row = excelSheet.CreateRow(rowIndex++);
                    ICell cell = row.CreateCell(2);
                    cell.CellStyle = centerCellStyle;
                    cell.SetCellValue("TRUNG TÂM KIỂM SOÁT BỆNH TẬT TP");
                    //
                    row = excelSheet.CreateRow(rowIndex++);
                    cell = row.CreateCell(2);
                    cell.CellStyle = centerBoldCellStyle;
                    cell.SetCellValue("KHOA XÉT NGHIỆM");
                    #endregion

                    #region Title
                    // add blank row
                    rowIndex++;
                    //
                    row = excelSheet.CreateRow(rowIndex++);
                    cell = row.CreateCell(2);
                    cell.CellStyle = centerBoldCellStyle;
                    cell.SetCellValue("PHIẾU GỬI MẪU XÉT NGHIỆM: COVID-19");
                    //
                    row = excelSheet.CreateRow(rowIndex++);
                    cell = row.CreateCell(2);
                    cell.CellStyle = centerCellStyle;
                    cell.SetCellValue("ĐỊA ĐIỂM LẤY MẪU: " + locationAddress);
                    #endregion

                    #region Common Info
                    // add blank row
                    rowIndex++;
                    //
                    row = excelSheet.CreateRow(rowIndex++);
                    cell = row.CreateCell(0);
                    cell.CellStyle = leftCellStyle;
                    cell.SetCellValue("Ngày thực hiện: " + dateTaken.ToString("dd/MM/yyyy"));
                    //
                    cell = row.CreateCell(5);
                    cell.CellStyle = leftCellStyle;
                    cell.SetCellValue("Nhân viên thực hiện: ");
                    //
                    row = excelSheet.CreateRow(rowIndex++);
                    cell = row.CreateCell(0);
                    cell.CellStyle = leftCellStyle;
                    cell.SetCellValue("Ngày gửi mẫu: ………………");
                    #endregion

                    #region Table Header
                    // Header
                    row = excelSheet.CreateRow(rowIndex++);
                    row = excelSheet.CreateRow(rowIndex++);
                    int cellIndex = 0;
                    // TT
                    excelSheet.AddMergedRegion(new CellRangeAddress(rowIndex - 1, rowIndex, cellIndex, cellIndex));
                    cell = row.CreateCell(cellIndex++);
                    cell.SetCellValue("STT");
                    cell.CellStyle = tableHeaderStyle;
                    // Mã phiếu hẹn
                    excelSheet.AddMergedRegion(new CellRangeAddress(rowIndex - 1, rowIndex, cellIndex, cellIndex));
                    cell = row.CreateCell(cellIndex++);
                    cell.SetCellValue("Mã phiếu hẹn");
                    cell.CellStyle = tableHeaderStyle;
                    // Họ tên
                    excelSheet.AddMergedRegion(new CellRangeAddress(rowIndex - 1, rowIndex, cellIndex, cellIndex));
                    cell = row.CreateCell(cellIndex++);
                    cell.SetCellValue("Họ tên");
                    cell.CellStyle = tableHeaderStyle;
                    // Ngày tháng năm sinh
                    excelSheet.AddMergedRegion(new CellRangeAddress(rowIndex - 1, rowIndex - 1, cellIndex, cellIndex + 1));
                    cell = row.CreateCell(cellIndex++);
                    cell.SetCellValue("Ngày tháng năm sinh");
                    cell.CellStyle = tableHeaderStyle;
                    cell = row.CreateCell(cellIndex++);
                    cell.CellStyle = tableHeaderStyle;
                    // Số passport
                    excelSheet.AddMergedRegion(new CellRangeAddress(rowIndex - 1, rowIndex, cellIndex, cellIndex));
                    cell = row.CreateCell(cellIndex++);
                    cell.SetCellValue("Số passport");
                    cell.CellStyle = tableHeaderStyle;
                    // Quốc tịch
                    excelSheet.AddMergedRegion(new CellRangeAddress(rowIndex - 1, rowIndex, cellIndex, cellIndex));
                    cell = row.CreateCell(cellIndex++);
                    cell.SetCellValue("Quốc tịch");
                    cell.CellStyle = tableHeaderStyle;
                    // Địa chỉ tại VN
                    excelSheet.AddMergedRegion(new CellRangeAddress(rowIndex - 1, rowIndex, cellIndex, cellIndex));
                    cell = row.CreateCell(cellIndex++);
                    cell.SetCellValue("Địa chỉ tại VN");
                    cell.CellStyle = tableHeaderStyle;
                    // SĐT
                    excelSheet.AddMergedRegion(new CellRangeAddress(rowIndex - 1, rowIndex, cellIndex, cellIndex));
                    cell = row.CreateCell(cellIndex++);
                    cell.SetCellValue("SĐT");
                    cell.CellStyle = tableHeaderStyle;
                    // Ngày giờ bay
                    excelSheet.AddMergedRegion(new CellRangeAddress(rowIndex - 1, rowIndex, cellIndex, cellIndex));
                    cell = row.CreateCell(cellIndex++);
                    cell.SetCellValue("Ngày giờ bay");
                    cell.CellStyle = tableHeaderStyle;
                    // Nơi đến
                    excelSheet.AddMergedRegion(new CellRangeAddress(rowIndex - 1, rowIndex, cellIndex, cellIndex));
                    cell = row.CreateCell(cellIndex++);
                    cell.SetCellValue("Nơi đến");
                    cell.CellStyle = tableHeaderStyle;
                    // Ngày giờ đến
                    excelSheet.AddMergedRegion(new CellRangeAddress(rowIndex - 1, rowIndex, cellIndex, cellIndex));
                    cell = row.CreateCell(cellIndex++);
                    cell.SetCellValue("Ngày giờ đến");
                    cell.CellStyle = tableHeaderStyle;
                    //
                    //
                    row = excelSheet.CreateRow(rowIndex++);
                    for (int i = 0; i < 11; i++)
                    {
                        cell = row.CreateCell(i);
                        cell.CellStyle = tableHeaderStyle;
                        if (i == 2)
                            cell.SetCellValue("Nam");
                        if (i == 3)
                            cell.SetCellValue("Nữ");
                    }
                    #endregion

                    #region Table Data
                    int stt = 0;
                    foreach (var item in rs.ToList())
                    {
                        stt++;
                        int dataIndex = 0;
                        row = excelSheet.CreateRow(rowIndex++);
                        // Stt
                        cell = row.CreateCell(dataIndex++);
                        cell.SetCellValue(stt);
                        cell.CellStyle = tableDataStyle;
                        // Stt
                        cell = row.CreateCell(dataIndex++);
                        cell.SetCellValue(item.Interval.NumId);
                        cell.CellStyle = tableDataStyle;
                        // Họ tên
                        cell = row.CreateCell(dataIndex++);
                        cell.SetCellValue(item.Customer.Fullname);
                        cell.CellStyle = tableDataStyle;
                        // Nam/Nữ Năm sinh
                        if (item != null && item.Customer != null)
                        {
                            // Nam
                            cell = row.CreateCell(dataIndex++);
                            cell.CellStyle = tableDataStyle;
                            cell.SetCellValue(item.Customer.Gender == true ? item.Customer.BirthDate.ToString("dd/MM/yyyy") : "");
                            // Nữ
                            cell = row.CreateCell(dataIndex++);
                            cell.CellStyle = tableDataStyle;
                            cell.SetCellValue(item.Customer.Gender == false ? item.Customer.BirthDate.ToString("dd/MM/yyyy") : "");
                        }
                        // Passport
                        cell = row.CreateCell(dataIndex++);
                        cell.SetCellValue(item.Customer.PassportNumber);
                        cell.CellStyle = tableDataStyle;
                        // Quốc tịch
                        cell = row.CreateCell(dataIndex++);
                        cell.SetCellValue(item.Customer.Nation);
                        cell.CellStyle = tableDataStyle;
                        // Địa chỉ tại VN
                        cell = row.CreateCell(dataIndex++);
                        cell.SetCellValue(item.Customer.GetFullAddress());
                        cell.CellStyle = tableDataStyle;
                        // SĐT
                        cell = row.CreateCell(dataIndex++);
                        cell.SetCellValue(item.Customer.Phone);
                        cell.CellStyle = tableDataStyle;
                        // Ngày giờ bay
                        cell = row.CreateCell(dataIndex++);
                        cell.SetCellValue(item.ExitInformation.ExitingDate.ToString("dd/MM/yyyy HH:mm"));
                        cell.CellStyle = tableDataStyle;
                        // Nơi đến
                        cell = row.CreateCell(dataIndex++);
                        cell.SetCellValue(item.ExitInformation.Destination);
                        cell.CellStyle = tableDataStyle;
                        // Ngày giờ đến
                        cell = row.CreateCell(dataIndex++);
                        cell.SetCellValue(item.ExitInformation.EntryingDate.ToString("dd/MM/yyyy HH:mm"));
                        cell.CellStyle = tableDataStyle;
                    }
                    #endregion
                    // sizing
                    cellIndex = 0;
                    // STT
                    excelSheet.SetColumnWidth(cellIndex++, 256*5);
                    // Họ tên
                    excelSheet.SetColumnWidth(cellIndex++, 256*25);
                    // Nam
                    excelSheet.SetColumnWidth(cellIndex++, 256*15);
                    // Nữ
                    excelSheet.SetColumnWidth(cellIndex++, 256*15);
                    // Số Passport
                    excelSheet.SetColumnWidth(cellIndex++, 256*15);
                    // Quốc tịch
                    excelSheet.SetColumnWidth(cellIndex++, 256*15);
                    // Địa chỉ tại VN
                    excelSheet.SetColumnWidth(cellIndex++, 256*30);
                    // SDT
                    excelSheet.SetColumnWidth(cellIndex++, 256*15);
                    // Ngày giờ bay
                    excelSheet.SetColumnWidth(cellIndex++, 256*25);
                    // Nơi đến
                    excelSheet.SetColumnWidth(cellIndex++, 256*15);
                    // Ngày giờ đến
                    excelSheet.SetColumnWidth(cellIndex++, 256*25);
                    workbook.Write(ms);
                    return new ResultModel()
                    {
                        Succeed = true,
                        Data = ms.ToArray(),
                    };
                }
            }
            catch (Exception e)
            {
                result.Succeed = false;
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
                result.ErrorMessage += Environment.NewLine + "StackTrace:" + e.StackTrace;
            }
            return result;
        }

        public async Task<ResultModel> VaccReport(Guid unitId, DateTime fromDate, DateTime toDate)
        {
            var result = new ResultModel();
            try
            {
                var basefilter = Builders<Vaccination>.Filter.Empty;
                var hospitalIdFilter = Builders<Vaccination>.Filter.Eq(mt => mt.Unit.Id, unitId);
                basefilter = basefilter & hospitalIdFilter;
                var dateFilter = Builders<Vaccination>.Filter.Where(mt => mt.Date >= fromDate.Date && mt.Date <= toDate.Date);
                basefilter = basefilter & dateFilter;
                var query = await _dbContext.Vaccinations.FindAsync(basefilter);
                var rs = await query.ToListAsync();
                if (rs.Count == 0)
                {
                    throw new Exception("Không có mẫu.");
                }
                var unit = rs.FirstOrDefault().Unit;
                using (var ms = new MemoryStream())
                {

                    IWorkbook workbook = new XSSFWorkbook();
                    ISheet excelSheet = workbook.CreateSheet("Danh sach tc");

                    #region Styling
                    // Fonts
                    // boldFont
                    IFont boldFont = workbook.CreateFont();
                    boldFont.IsBold = true;
                    boldFont.FontHeightInPoints = 13;
                    // table header font
                    IFont tableHeaderFont = workbook.CreateFont();
                    tableHeaderFont.IsBold = true;
                    tableHeaderFont.FontHeightInPoints = 11;
                    // CellStyles
                    // normal left align cell
                    ICellStyle leftCellStyle = workbook.CreateCellStyle();
                    leftCellStyle.Alignment = HorizontalAlignment.Left;
                    // normal center cell
                    ICellStyle centerCellStyle = workbook.CreateCellStyle();
                    centerCellStyle.Alignment = HorizontalAlignment.Center;
                    // bold center cell
                    ICellStyle centerBoldCellStyle = workbook.CreateCellStyle();
                    centerBoldCellStyle.Alignment = HorizontalAlignment.Center;
                    centerBoldCellStyle.SetFont(boldFont);
                    // table header cell style
                    ICellStyle tableHeaderStyle = workbook.CreateCellStyle();
                    tableHeaderStyle.Alignment = HorizontalAlignment.Center;
                    tableHeaderStyle.VerticalAlignment = VerticalAlignment.Center;
                    tableHeaderStyle.SetFont(tableHeaderFont);
                    tableHeaderStyle.BorderTop = BorderStyle.Thin;
                    tableHeaderStyle.BorderBottom = BorderStyle.Thin;
                    tableHeaderStyle.BorderRight = BorderStyle.Thin;
                    tableHeaderStyle.BorderLeft = BorderStyle.Thin;
                    tableHeaderStyle.WrapText = true;
                    // table data style
                    ICellStyle tableDataStyle = workbook.CreateCellStyle();
                    tableDataStyle.Alignment = HorizontalAlignment.Center;
                    tableDataStyle.VerticalAlignment = VerticalAlignment.Center;
                    tableDataStyle.BorderTop = BorderStyle.Thin;
                    tableDataStyle.BorderBottom = BorderStyle.Thin;
                    tableDataStyle.BorderRight = BorderStyle.Thin;
                    tableDataStyle.BorderLeft = BorderStyle.Thin;
                    tableDataStyle.WrapText = true;
                    #endregion

                    #region Header
                    int rowIndex = 0;
                    IRow row = excelSheet.CreateRow(rowIndex++);
                    ICell cell = row.CreateCell(2);
                    cell.CellStyle = centerCellStyle;
                    cell.SetCellValue(unit.Name);
                    //
                    row = excelSheet.CreateRow(rowIndex++);
                    cell = row.CreateCell(2);
                    cell.CellStyle = centerBoldCellStyle;
                    //cell.SetCellValue(unit.);
                    #endregion

                    #region Title
                    // add blank row
                    rowIndex++;
                    //
                    row = excelSheet.CreateRow(rowIndex++);
                    cell = row.CreateCell(2);
                    cell.CellStyle = centerBoldCellStyle;
                    cell.SetCellValue("DANH SÁCH TIÊM CHỦNG");
                    //
                    row = excelSheet.CreateRow(rowIndex++);
                    cell = row.CreateCell(2);
                    cell.CellStyle = centerCellStyle;
                    cell.SetCellValue("ĐỊA ĐIỂM TIÊM: " + unit.Address);
                    #endregion

                    #region Common Info
                    // add blank row
                    rowIndex++;
                    //
                    row = excelSheet.CreateRow(rowIndex++);
                    cell = row.CreateCell(0);
                    cell.CellStyle = leftCellStyle;
                    cell.SetCellValue("Từ ngày: " + fromDate.ToString("dd/MM/yyyy"));
                    cell = row.CreateCell(3);
                    cell.CellStyle = leftCellStyle;
                    cell.SetCellValue("Tới ngày: " + toDate.ToString("dd/MM/yyyy"));
                    #endregion

                    #region Table Header
                    // Header
                    row = excelSheet.CreateRow(rowIndex++);
                    row = excelSheet.CreateRow(rowIndex++);
                    int cellIndex = 0;
                    // TT
                    excelSheet.AddMergedRegion(new CellRangeAddress(rowIndex - 1, rowIndex, cellIndex, cellIndex));
                    cell = row.CreateCell(cellIndex++);
                    cell.SetCellValue("STT");
                    cell.CellStyle = tableHeaderStyle;
                    // Mã phiếu hẹn
                    excelSheet.AddMergedRegion(new CellRangeAddress(rowIndex - 1, rowIndex, cellIndex, cellIndex));
                    cell = row.CreateCell(cellIndex++);
                    cell.SetCellValue("Mã phiếu hẹn");
                    cell.CellStyle = tableHeaderStyle;
                    // Họ tên
                    excelSheet.AddMergedRegion(new CellRangeAddress(rowIndex - 1, rowIndex, cellIndex, cellIndex));
                    cell = row.CreateCell(cellIndex++);
                    cell.SetCellValue("Họ tên");
                    cell.CellStyle = tableHeaderStyle;
                    // Ngày tháng năm sinh
                    excelSheet.AddMergedRegion(new CellRangeAddress(rowIndex - 1, rowIndex - 1, cellIndex, cellIndex + 1));
                    cell = row.CreateCell(cellIndex++);
                    cell.SetCellValue("Ngày tháng năm sinh");
                    cell.CellStyle = tableHeaderStyle;
                    cell = row.CreateCell(cellIndex++);
                    cell.CellStyle = tableHeaderStyle;
                    // Mã tiêm chủng
                    excelSheet.AddMergedRegion(new CellRangeAddress(rowIndex - 1, rowIndex, cellIndex, cellIndex));
                    cell = row.CreateCell(cellIndex++);
                    cell.SetCellValue("Mã tiêm chủng");
                    cell.CellStyle = tableHeaderStyle;
                    // Địa chỉ 
                    excelSheet.AddMergedRegion(new CellRangeAddress(rowIndex - 1, rowIndex, cellIndex, cellIndex));
                    cell = row.CreateCell(cellIndex++);
                    cell.SetCellValue("Địa chỉ");
                    cell.CellStyle = tableHeaderStyle;
                    // Họ tên người đi cùng
                    excelSheet.AddMergedRegion(new CellRangeAddress(rowIndex - 1, rowIndex, cellIndex, cellIndex));
                    cell = row.CreateCell(cellIndex++);
                    cell.SetCellValue("Họ tên người đi cùng");
                    cell.CellStyle = tableHeaderStyle;
                    // Mối quan hệ
                    excelSheet.AddMergedRegion(new CellRangeAddress(rowIndex - 1, rowIndex, cellIndex, cellIndex));
                    cell = row.CreateCell(cellIndex++);
                    cell.SetCellValue("Mối quan hệ");
                    cell.CellStyle = tableHeaderStyle;
                    // SĐT
                    excelSheet.AddMergedRegion(new CellRangeAddress(rowIndex - 1, rowIndex, cellIndex, cellIndex));
                    cell = row.CreateCell(cellIndex++);
                    cell.SetCellValue("SĐT");
                    cell.CellStyle = tableHeaderStyle;
                    // Mũi tiêm
                    excelSheet.AddMergedRegion(new CellRangeAddress(rowIndex - 1, rowIndex, cellIndex, cellIndex));
                    cell = row.CreateCell(cellIndex++);
                    cell.SetCellValue("Mũi tiêm");
                    cell.CellStyle = tableHeaderStyle;
                    // Ngày tiêm
                    excelSheet.AddMergedRegion(new CellRangeAddress(rowIndex - 1, rowIndex, cellIndex, cellIndex));
                    cell = row.CreateCell(cellIndex++);
                    cell.SetCellValue("Ngày tiêm");
                    cell.CellStyle = tableHeaderStyle;
                    // Loại hình
                    excelSheet.AddMergedRegion(new CellRangeAddress(rowIndex - 1, rowIndex, cellIndex, cellIndex));
                    cell = row.CreateCell(cellIndex++);
                    cell.SetCellValue("Loại hình");
                    cell.CellStyle = tableHeaderStyle;
                    //
                    row = excelSheet.CreateRow(rowIndex++);
                    for (int i = 0; i <= 12; i++)
                    {
                        cell = row.CreateCell(i);
                        cell.CellStyle = tableHeaderStyle;
                        if (i == 3)
                            cell.SetCellValue("Nam");
                        if (i == 4)
                            cell.SetCellValue("Nữ");
                    }
                    #endregion

                    #region Table Data
                    int stt = 0;
                    foreach (var item in rs.ToList())
                    {
                        stt++;
                        int dataIndex = 0;
                        row = excelSheet.CreateRow(rowIndex++);
                        // Stt
                        cell = row.CreateCell(dataIndex++);
                        cell.SetCellValue(stt);
                        cell.CellStyle = tableDataStyle;
                        // Id
                        cell = row.CreateCell(dataIndex++);
                        cell.SetCellValue(item.Interval.NumId);
                        cell.CellStyle = tableDataStyle;
                        // Họ tên
                        cell = row.CreateCell(dataIndex++);
                        cell.SetCellValue(item.Customer.Fullname);
                        cell.CellStyle = tableDataStyle;
                        // Nam/Nữ Năm sinh
                        if (item != null && item.Customer != null)
                        {
                            // Nam
                            cell = row.CreateCell(dataIndex++);
                            cell.CellStyle = tableDataStyle;
                            cell.SetCellValue(item.Customer.Gender == true ? item.Customer.BirthDate.ToString("dd/MM/yyyy") : "");
                            // Nữ
                            cell = row.CreateCell(dataIndex++);
                            cell.CellStyle = tableDataStyle;
                            cell.SetCellValue(item.Customer.Gender == false ? item.Customer.BirthDate.ToString("dd/MM/yyyy") : "");
                        }
                        // Mã tiêm chủng
                        cell = row.CreateCell(dataIndex++);
                        cell.SetCellValue(item.Customer.VaccinationCode);
                        cell.CellStyle = tableDataStyle;
                        // Địa chỉ tại VN
                        cell = row.CreateCell(dataIndex++);
                        cell.SetCellValue(item.Customer.GetFullAddress());
                        cell.CellStyle = tableDataStyle;
                        // Họ tên người đi cùng
                        cell = row.CreateCell(dataIndex++);
                        cell.SetCellValue(item.Contacts.FirstOrDefault() != null ? item.Contacts.FirstOrDefault().Fullname : null);
                        cell.CellStyle = tableDataStyle;
                        // Mối quan hệ
                        cell = row.CreateCell(dataIndex++);
                        cell.SetCellValue(item.Contacts.FirstOrDefault() != null ? item.Contacts.FirstOrDefault().Relationship : null);
                        cell.CellStyle = tableDataStyle;
                        // SĐT
                        cell = row.CreateCell(dataIndex++);
                        cell.SetCellValue(item.Customer.Phone);
                        cell.CellStyle = tableDataStyle;
                        // Mũi tiêm
                        cell = row.CreateCell(dataIndex++);
                        cell.SetCellValue(item.Service.Name);
                        cell.CellStyle = tableDataStyle;
                        // Ngày tiêm
                        cell = row.CreateCell(dataIndex++);
                        cell.SetCellValue(item.Date.ToString("dd/MM/yyyy"));
                        cell.CellStyle = tableDataStyle;
                        // Loại hình
                        cell = row.CreateCell(dataIndex++);
                        cell.SetCellValue(item.ServiceType != null ? item.ServiceType.Name : null);
                        cell.CellStyle = tableDataStyle;
                    }
                    #endregion
                    // sizing
                    cellIndex = 0;
                    // STT
                    excelSheet.SetColumnWidth(cellIndex++, 256 * 5);
                    // Id
                    excelSheet.SetColumnWidth(cellIndex++, 256 * 5);
                    // Họ tên
                    excelSheet.SetColumnWidth(cellIndex++, 256 * 25);
                    // Nam
                    excelSheet.SetColumnWidth(cellIndex++, 256 * 15);
                    // Nữ
                    excelSheet.SetColumnWidth(cellIndex++, 256 * 15);
                    // mã tiêm
                    excelSheet.SetColumnWidth(cellIndex++, 256 * 15);
                    // Địa chỉ tại VN
                    excelSheet.SetColumnWidth(cellIndex++, 256 * 30);
                    // Họ tên
                    excelSheet.SetColumnWidth(cellIndex++, 256 * 25);
                    // MQH
                    excelSheet.SetColumnWidth(cellIndex++, 256 * 15);
                    // SDT
                    excelSheet.SetColumnWidth(cellIndex++, 256 * 20);
                    // Mũi tiêm
                    excelSheet.SetColumnWidth(cellIndex++, 256 * 30);
                    // Ngày tiêm
                    excelSheet.SetColumnWidth(cellIndex++, 256 * 25);
                    // Loại hình
                    excelSheet.SetColumnWidth(cellIndex++, 256 * 30);
                    //
                    workbook.Write(ms);
                    return new ResultModel()
                    {
                        Succeed = true,
                        Data = ms.ToArray(),
                    };
                }
            }
            catch (Exception e)
            {
                result.Succeed = false;
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
                result.ErrorMessage += Environment.NewLine + "StackTrace:" + e.StackTrace;
            }
            return result;
        }
    }
}
