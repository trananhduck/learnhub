﻿using LearnHub.Models;
using LearnHub.Services;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using LearnHub.Commands;
using Microsoft.Win32;
using Microsoft.EntityFrameworkCore;
using System.IO;
using LearnHub.Stores;

namespace LearnHub.ViewModels.ExportModalViewModels
{
    public class ExportResultViewModel : BaseViewModel
    {
        public IEnumerable<AcademicYear> Years { get; private set; }

        private AcademicYear _selectedYear;
        public AcademicYear SelectedYear
        {
            get => _selectedYear;
            set
            {
                _selectedYear = value;
                OnPropertyChanged(nameof(SelectedYear));
            }
        }
        private string _selectedSemester;
        public string SelectedSemester
        {
            get
            {
                return _selectedSemester;
            }
            set
            {
                _selectedSemester = value;
                OnPropertyChanged(nameof(SelectedSemester));
            }
        }


        private async void LoadYears()
        {
            Years = await GenericDataService<AcademicYear>.Instance.GetAll();
            OnPropertyChanged(nameof(Years));
        }


        public ICommand SubmitCommand { get; }
        public ICommand CancelCommand { get; }

        public ExportResultViewModel()
        {
            SubmitCommand = new RelayCommand(ExportToExcel);
            CancelCommand = new CancelCommand();
            LoadYears();
        }


        private double CalculateAverageScore(Score score)
        {
            if (score == null) return 0;

            double sum = 0;
            int count = 0;


            // Tính trung bình điểm, bao gồm TXScore, GKScore, CKScore
            if (!string.IsNullOrWhiteSpace(score.RegularScores))
            {
                double[] txScores = score.RegularScores.Split(' ')
                                            .Select(s => double.Parse(s.Trim()))
                                            .ToArray();
                sum += txScores.Average();
                count++;
            }

            if (score.MidTermScore.HasValue)
            {
                sum += score.MidTermScore.Value;
                count++;
            }

            if (score.FinalTermScore.HasValue)
            {
                sum += score.FinalTermScore.Value;
                count++;
            }

            return count > 0 ? sum / count : 0;
        }

        private string CalculateAcademicPerformance(IEnumerable<double> avgScores)
        {
            double min = avgScores.Min();
            double avgScore = avgScores.Average();
            if (avgScore >= 8 && min >= 6.5) return "Giỏi";
            else if (avgScore >= 6.5 && min >= 5) return "Khá";
            else if (avgScore >= 5 && min >= 3.5) return "Trung bình";
            else if (avgScore >= 3.5 && min >= 2) return "Yếu";
            else return "Kém";
        }

        private string CaculateConduct(string hk1, string hk2)
        {
            if (hk1 == null || hk2 == null)
            {
                ToastMessageViewModel.ShowWarningToast("Chưa nhập hạnh kiểm học kì.");
                return null;
            }
            Dictionary<string, int> conducts = new Dictionary<string, int>()
            {
                {"Tốt",5 },{"Khá",4},{"Trung bình", 3 },{"Yếu",2},{"Kém",1}
            };
            int result = (conducts[hk1] + 2 * conducts[hk2]) / 3;
            switch (result)
            {
                case 5: return "Tốt";
                case 4: return "Khá";
                case 3: return "Trung bình";
                case 2: return "Yếu";
                default: return "Kém";
            }
        }

        private async void ExportToExcel()
        {
            if (SelectedYear == null || SelectedSemester == null)
            {
                ToastMessageViewModel.ShowWarningToast("Chưa chọn đủ thông tin");
                return;
            }

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            try
            {
                // Tạo SaveFileDialog để người dùng chọn nơi lưu file
                var saveFileDialog = new SaveFileDialog
                {
                    Title = "Chọn nơi lưu file Excel",
                    Filter = "Excel Files (*.xlsx)|*.xlsx",
                    FileName = $"KQ_{SelectedYear.Name}_{SelectedSemester}.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true) // Nếu người dùng nhấn "Save"
                {
                    string filePath = saveFileDialog.FileName;

                    //lấy tất cả lớp của năm được chọn, xếp theo khối => tên

                    var classrooms = (await GenericDataService<Classroom>.Instance
                         .GetMany(e => e.YearId == SelectedYear.Id, include: query => query.Include(e => e.Grade)))
                         .OrderBy(e => e.Grade.Number)
                         .OrderBy(e => e.Name)
                         .ToList();

                    using (var package = new ExcelPackage())
                    {
                        //vòng lặp cho từng sheet
                        for (int k = 0; k < classrooms.Count; k++)
                        {
                            var classroom = classrooms[k];
                            var worksheet = package.Workbook.Worksheets.Add(classroom.Name);

                            // Định dạng chung cho ô
                            worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            worksheet.Cells["A1"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            worksheet.Cells["A1"].Style.WrapText = true;

                            worksheet.Row(1).Height = 100; // Tùy chỉnh chiều cao phù hợp

                            // Thêm tiêu đề các cột
                            //Tiêu đề cố định
                            var headers = new List<string>()
                          {
                              "STT",
                              "Họ và tên",

                          };
                            //Thêm các môn của khối vào tiêu đề

                            var subjectNames = (await GenericDataService<Subject>.Instance.GetMany(e => e.GradeId == classroom.GradeId))
                                .Select(e => e.Name)
                                .OrderBy(e => e)
                                .ToList();

                            headers.AddRange(subjectNames);
                            headers.Add("Trung bình");
                            headers.Add("Học lực");
                            headers.Add("Hạnh kiểm");


                            //Ghi các headers
                            for (int i = 0; i < headers.Count; i++)
                            {
                                worksheet.Cells[2, i + 1].Value = headers[i];
                                worksheet.Cells[2, i + 1].Style.Font.Bold = true; // In đậm tiêu đề
                                worksheet.Cells[2, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                worksheet.Cells[2, i + 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            }

                            // Thêm dữ liệu 
                            //Lấy ra danh sách học sinh của lớp ? <= từ studentId suy ra <= lấy studentplacment 

                            var students = (await GenericDataService<StudentPlacement>.Instance.Query(sp =>
                                sp.Where(sp => sp.ClassroomId == classroom.Id)
                                .Select(sp => sp.Student)))
                                .ToList();

                            //Duyệt từng học sinh
                            for (int i = 0; i < students.Count; i++)
                            {
                                worksheet.Cells[i + 3, 1].Value = i + 1; //STT
                                worksheet.Cells[i + 3, 2].Value = students[i].FullName; //Họ tên

                                if (SelectedSemester == "HK1" || SelectedSemester == "HK2")
                                {


                                    //lấy danh sách điểm của từng học sinh kèm theo môn
                                    var scores = await GenericDataService<Score>.Instance.GetMany(e => e.StudentId == students[i].Id && e.Semester == SelectedSemester,
                                        include: query => query.Include(e => e.Subject));

                                    for (int j = 3; j <= headers.Count - 3; j++)
                                    {
                                        //ở mỗi cột môn (bắt đàu từ 3), kiểm tra trong đống scores thằng nào có subject name trùng với tên cột 
                                        //thì tính trung bình rồi quăng vào ô đó
                                        string subjectName = worksheet.Cells[2, j].Value.ToString(); // tên môn (dòng 2)
                                        Score matchedScore = scores.FirstOrDefault(e => e.Subject.Name == subjectName); //điểm match với tên môn
                                        worksheet.Cells[i + 3, j].Value = Math.Round(CalculateAverageScore(matchedScore), 2);
                                    }

                                    //lấy ra semesterresult của học sinh
                                    var semesterResult = await GenericDataService<SemesterResult>.Instance.GetOne(e => e.StudentId == students[i].Id
                                    && e.YearId == SelectedYear.Id && e.Semester == SelectedSemester);

                                    var avgScores = scores.Select(CalculateAverageScore); //điểm trung bình của các môn
                                                                                          //các cột kết quả cuối cùng
                                    worksheet.Cells[i + 3, headers.Count - 2].Value = Math.Round(avgScores.Average(), 2); //tb cả kì
                                    worksheet.Cells[i + 3, headers.Count - 1].Value = semesterResult.AcademicPerformance ?? "Chưa có";
                                    worksheet.Cells[i + 3, headers.Count].Value = semesterResult.Conduct ?? "Chưa có";
                                    Console.WriteLine();
                                }
                                else if (SelectedSemester == "Cả năm")
                                {
                                    //Lấy điểm của hk1 và hk2
                                    //Do cả 2 kì học như nhau nên mặc định chiều dài của 2 mảng này bằng nhau đối vớ 1 sinh viên ?
                                    var scores1 = (await GenericDataService<Score>.Instance.GetMany(e => e.StudentId == students[i].Id && e.Semester == "HK1",
                                    include: query => query.Include(e => e.Subject))).ToList();
                                    var scores2 = (await GenericDataService<Score>.Instance.GetMany(e => e.StudentId == students[i].Id && e.Semester == "HK2",
                                    include: query => query.Include(e => e.Subject))).ToList();


                                    if (scores1.Count != scores2.Count)
                                    {
                                        ToastMessageViewModel.ShowWarningToast("Chưa đủ thông tin để xuất kết quả cả năm");
                                        return;
                                    }

                                    for (int j = 3; j <= headers.Count - 3; j++)
                                    {
                                        //ở mỗi cột môn (bắt đàu từ 3), kiểm tra trong đống scores thằng nào có subject name trùng với tên cột 
                                        //thì tính trung bình rồi quăng vào ô đó
                                        string subjectName = worksheet.Cells[2, j].Value.ToString(); // tên môn (dòng 2)
                                        Score matchedScore1 = scores1.FirstOrDefault(e => e.Subject.Name == subjectName); //điểm match với tên môn
                                        Score matchedScore2 = scores2.FirstOrDefault(e => e.Subject.Name == subjectName); //điểm match với tên môn
                                        //tính điểm tb cả năm (hk2 hệ số 2)
                                        double yearAvgScore = (CalculateAverageScore(matchedScore1) + CalculateAverageScore(matchedScore2) * 2) / 3;
                                        worksheet.Cells[i + 3, j].Value = Math.Round(yearAvgScore, 2);
                                    }


                                    //Điểm tb từng môn (do cùng số lượng và sắp xêp như nhau => khả năng cao là trên dưới tương ứng)
                                    var avgScores1 = scores1.OrderBy(e => e.SubjectId).Select(CalculateAverageScore).ToList();
                                    var avgScores2 = scores2.OrderBy(e => e.SubjectId).Select(CalculateAverageScore).ToList();

                                    var yearAvgScores = new List<double>();
                                    for (int j = 0; j < avgScores1.Count; j++)
                                    {
                                        double yearAvgScore = (avgScores1[j] + avgScores2[j] * 2) / 3;
                                        yearAvgScores.Add(yearAvgScore);
                                        Console.WriteLine();
                                    }


                                    var semesterResult1 = await GenericDataService<SemesterResult>.Instance.GetOne(e => e.StudentId == students[i].Id
                                  && e.YearId == SelectedYear.Id && e.Semester == "HK1");
                                    var semesterResult2 = await GenericDataService<SemesterResult>.Instance.GetOne(e => e.StudentId == students[i].Id
                                  && e.YearId == SelectedYear.Id && e.Semester == "HK2");

                                    worksheet.Cells[i + 3, headers.Count - 2].Value = Math.Round(yearAvgScores.Average(), 2); //tb cả kì
                                    worksheet.Cells[i + 3, headers.Count - 1].Value = CalculateAcademicPerformance(yearAvgScores) ?? "Chưa có";
                                    worksheet.Cells[i + 3, headers.Count].Value = CaculateConduct(semesterResult1.Conduct, semesterResult2.Conduct) ?? "Chưa có";
                                  
                                }

                            }

                            // Vẽ border cho tất cả các ô chứa dữ liệu (bảng không cố định)
                            var totalRows = students.Count + 2; // Bao gồm title, header và dữ liệu
                            var totalColumns = headers.Count;
                            var dataRange = worksheet.Cells[1, 1, totalRows, totalColumns];
                            dataRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            dataRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            dataRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                            dataRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                            dataRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            dataRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                            //Các ô họ tên sẽ căn trái (chỉ định dạng nếu có dữ liệu, nếu không sẽ nằm ngoài phạm vi)
                            if (students.Count > 0)
                            {
                                var nameRange = worksheet.Cells[3, 2, totalRows, 2];
                                nameRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            }


                            // Định dạng tiêu đề (merge các cột tương ứng với số lượng header)
                            worksheet.Cells[1, 1, 1, headers.Count].Merge = true;

                            //Nội dung ô title
                            var richText = worksheet.Cells["A1"].RichText;
                            var title = richText.Add("Kết quả học tập\n");
                            title.Bold = true;
                            title.Size = 20;

                            var content = richText.Add($"Năm học: {SelectedYear.Name}\nHọc kì: {SelectedSemester}\nLớp: {classroom.Name}");
                            content.Size = 14;



                            // Tự động căn chỉnh chiều rộng theo nội dung
                            worksheet.Cells[dataRange.Address].AutoFitColumns();


                            worksheet.Column(1).Width = 5; //STT
                            worksheet.Column(2).Width = 30; // Họ và tên

                            //các cột còn lại
                            for (int col = 3; col <= totalColumns; col++)
                            {
                                worksheet.Column(col).Width = 15;
                            }

                            // Lưu file Excel
                        }
                        File.WriteAllBytes(filePath, package.GetAsByteArray());
                    }

                    // Thông báo thành công
                    ModalNavigationStore.Instance.Close();
                    ToastMessageViewModel.ShowSuccessToast($"Xuất dữ liệu thành công vào file: {filePath}");
                }
            }
            catch (Exception ex)
            {
                ToastMessageViewModel.ShowErrorToast($"Xuất dữ liệu thất bại: {ex.Message}");
            }
        }
    }
}