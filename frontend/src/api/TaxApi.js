import api from "./AxiosClient";


export const getDeclarations = () =>{
    return api.get("/tax/declarations");
}

export const getDeclarationDetail = (id) =>{
    return api.get(`/tax/declarations/${id}`);
}

export const approveDeclaration = (id) =>{
    return api.put(`/tax/declarations/${id}/approve`);
}

export const exportDeclarationPdf = (id) =>{
    return api.get(`/tax/declarations/${id}/export-pdf`, {
        responseType: "blob",
    });
}

export const getUnreportedInvoices = (params) =>{
    return api.get("/tax/invoices-unreported", { params });
}

export const generateDeclaration = (data) =>{
    return api.post("/tax/declarations/generate", data);
}

export const deleteDeclaration = async (id) => {
  return await api.delete(`/tax/${id}`);
};
