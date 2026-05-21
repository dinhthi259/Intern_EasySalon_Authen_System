import React, { useState, useEffect } from "react";
import KPICard from "./KPICard";
import RevenueChart from "./RevenueChart";
import CategoryChart from "./CategoryChart";
import TopProductsTable from "./TopProductsTable";
import styles from "./StatisticsDashboard.module.scss";
import {
  exportToExcel,
  exportToPdf,
  getKpiCards,
} from "../../../api/StatisticsApi";

const StatisticsDashboard = () => {
  const [kpiCards, setKpiCards] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [activeTab, setActiveTab] = useState("overview");

  useEffect(() => {
    fetchKPICards();
  }, []);

  const fetchKPICards = async () => {
    try {
      setLoading(true);
      const response = await getKpiCards();
      if (response.data.success) {
        setKpiCards(response.data.data);
        setError(null);
      }
    } catch (err) {
      setError("Failed to load dashboard data");
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleExportExcel = async () => {
    try {
      const response = await exportToExcel({
        format: "EXCEL",
        statisticsType: "SUMMARY",
        reportTitle: "Monthly Statistics Report",
      });

      // Create a blob and download it
      const url = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement("a");
      link.href = url;
      link.setAttribute(
        "download",
        `statistics_${new Date().toISOString()}.xlsx`,
      );
      document.body.appendChild(link);
      link.click();
      link.remove();
    } catch (err) {
      console.error("Export failed:", err);
      alert("Failed to export Excel file");
    }
  };

  const handleExportPdf = async () => {
    try {
      const response = await exportToPdf({
        format: "PDF",
        statisticsType: "SUMMARY",
        reportTitle: "Monthly Statistics Report",
      });

      // Create a blob and download it
      const url = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement("a");
      link.href = url;
      link.setAttribute(
        "download",
        `statistics_${new Date().toISOString()}.pdf`,
      );
      document.body.appendChild(link);
      link.click();
      link.remove();
    } catch (err) {
      console.error("Export failed:", err);
      alert("Failed to export PDF file");
    }
  };

  if (loading) {
    return (
      <div className={styles.dashboard}>
        <div className={styles.loadingContainer}>
          <div className={styles.spinner} />
          <p>Loading dashboard...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className={styles.dashboard}>
        <div className={styles.errorContainer}>
          <p>{error}</p>
          <button onClick={fetchKPICards} className={styles.retryButton}>
            Retry
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className={styles.dashboard}>
      <div className={styles.header}>
        <h2>Báo cáo thống kê</h2>
      </div>

      {/* KPI Cards */}
      <div className={styles.kpiGrid}>
        {kpiCards.map((card, index) => (
          <KPICard
            key={index}
            title={card.title}
            value={card.value}
            unit={card.unit}
            changePercentage={card.changePercentage}
            trend={card.trend}
            color={card.color}
          />
        ))}
      </div>

      <RevenueChart />
    </div>
  );
};

export default StatisticsDashboard;
