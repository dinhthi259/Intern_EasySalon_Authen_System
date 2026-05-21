import api from "./AxiosClient";

export const getAllInvoices = (params) => {
  return api.get("/admin/invoices", { params });
};

export const getInvoices = () => {
  return api.get("/admin/invoices");
};
  
export const getInvoiceById = (id) => {
  return api.get(`/admin/invoices/${id}`);
};
 