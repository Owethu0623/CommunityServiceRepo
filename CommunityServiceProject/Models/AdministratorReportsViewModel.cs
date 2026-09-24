using System;
using System.Collections.Generic;
using CommunityServiceProject.Models;

namespace CommunityServiceProject.ViewModels
{
    public class AdministratorReportsViewModel
    {
        // ================================
        // FILTERS
        // ================================

        public string SelectedStatus { get; set; }
        public int? SelectedCategoryId { get; set; }
        public int? SelectedWardId { get; set; }
        public string SelectedPriority { get; set; }
        public int? SelectedTechnicianId { get; set; }
        public DateTime? SelectedDateFrom { get; set; }
        public DateTime? SelectedDateTo { get; set; }

        public string SelectedReportOption { get; set; }

        // ================================
        // FILTER OPTIONS
        // ================================

        public List<Category> Categories { get; set; }
        public List<Ward> Wards { get; set; }
        public List<Technician> Technicians { get; set; }

        // ================================
        // SUMMARY
        // ================================

        public int TotalRequests { get; set; }
        public int PendingRequests { get; set; }
        public int CompletedRequests { get; set; }
        public int RejectedRequests { get; set; }
        public int CategoryCount { get; set; }

        // ================================
        // REPORTS
        // ================================

        public List<CategoryReportRow> RequestsByCategory { get; set; }
        public List<WardReportRow> RequestsByWard { get; set; }
        public List<StatusReportRow> RequestsByStatus { get; set; }
        public List<PriorityReportRow> RequestsByPriority { get; set; }
        public List<TechnicianReportRow> TechnicianPerformance { get; set; }

        public double AverageResolutionDays { get; set; }
        public int ResolvedRequests { get; set; }
        public double ResolutionRate { get; set; }
        public int OpenOverSevenDays { get; set; }

        public List<CategoryResolutionReportRow>
            CategoryResolutionPerformance
        { get; set; }

        // ================================
        // CONSTRUCTOR
        // ================================

        public AdministratorReportsViewModel()
        {
            Categories = new List<Category>();
            Wards = new List<Ward>();
            Technicians = new List<Technician>();

            RequestsByCategory = new List<CategoryReportRow>();
            RequestsByWard = new List<WardReportRow>();
            RequestsByStatus = new List<StatusReportRow>();
            RequestsByPriority = new List<PriorityReportRow>();
            TechnicianPerformance = new List<TechnicianReportRow>();
            CategoryResolutionPerformance =
                new List<CategoryResolutionReportRow>();

            SelectedReportOption =
                "ServiceResolutionPerformance";
        }
    }

    // ============================================
    // CATEGORY REPORT
    // ============================================

    public class CategoryReportRow
    {
        public string CategoryName { get; set; }
        public int RequestCount { get; set; }
        public double Percentage { get; set; }
    }

    // ============================================
    // WARD REPORT
    // ============================================

    public class WardReportRow
    {
        public string WardNumber { get; set; }
        public string WardName { get; set; }
        public int RequestCount { get; set; }
        public double Percentage { get; set; }
    }

    // ============================================
    // STATUS REPORT
    // ============================================

    public class StatusReportRow
    {
        public string Status { get; set; }
        public int RequestCount { get; set; }
        public double Percentage { get; set; }
    }

    // ============================================
    // PRIORITY REPORT
    // ============================================

    public class PriorityReportRow
    {
        public string Priority { get; set; }
        public int RequestCount { get; set; }
        public double Percentage { get; set; }
    }

    // ============================================
    // TECHNICIAN REPORT
    // ============================================

    public class TechnicianReportRow
    {
        public string TechnicianName { get; set; }
        public int TotalRequests { get; set; }
        public int CompletedRequests { get; set; }
        public int ActiveRequests { get; set; }
        public double CompletionRate { get; set; }
    }

    // ============================================
    // RESOLUTION BY CATEGORY
    // ============================================

    public class CategoryResolutionReportRow
    {
        public string CategoryName { get; set; }
        public int CompletedRequests { get; set; }
        public double AverageResolutionDays { get; set; }
    }
}