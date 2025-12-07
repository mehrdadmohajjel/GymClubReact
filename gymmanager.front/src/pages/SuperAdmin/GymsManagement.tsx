import React, { useEffect, useState } from "react";
import { Card, Table, Button, Modal, Form, Input, message, Popconfirm } from "antd";
import api from "../../api/axios";

const GymsManagement: React.FC = () => {
    const [list, setList] = useState<any[]>([]);
    const [loading, setLoading] = useState(false);
    const [visible, setVisible] = useState(false);
    const [editing, setEditing] = useState<any>(null);
    const [form] = Form.useForm();

    const load = async () => {
        setLoading(true);
        try {
            const res = await api.get("/gyms");
            setList(res.data || []);
        } finally { setLoading(false); }
    };

    useEffect(() => { load(); }, []);

    const openAdd = () => { setEditing(null); form.resetFields(); setVisible(true); };
    const openEdit = (r: any) => { setEditing(r); form.setFieldsValue(r); setVisible(true); };

    const save = async () => {
        const vals = await form.validateFields();
        try {
            if (editing) {
                await api.put(`/gyms/${editing.id}`, vals);
                message.success("ویرایش شد");
            } else {
                await api.post("/gyms/request", vals);
                message.success("درخواست ثبت شد");
            }
            setVisible(false);
            load();
        } catch (err: any) { message.error(err?.response?.data?.error ?? "خطا"); }
    };

    const del = async (id: string) => {
        try {
            await api.delete(`/gyms/${id}`);
            message.success("حذف شد");
            load();
        } catch (err: any) { message.error("خطا"); }
    };

    const columns = [
        { title: "نام", dataIndex: "name" },
        { title: "آدرس", dataIndex: "address" },
        { title: "تلفن", dataIndex: "phone" },
        { title: "وضعیت", dataIndex: "isApproved", render: (v: boolean) => v ? "تأیید" : "در انتظار" },
        {
            title: "عملیات", render: (_: any, r: any) => (
                <>
                    <Button onClick={() => openEdit(r)} size="small">ویرایش</Button>
                    <Popconfirm title="آیا حذف می‌کنید؟" onConfirm={() => del(r.id)}>
                        <Button danger size="small" style={{ marginLeft: 8 }}>حذف</Button>
                    </Popconfirm>
                </>
            )
        }
    ];

    return (
        <Card title="مدیریت باشگاه‌ها" extra={<Button onClick={openAdd}>افزودن/درخواست</Button>}>
            <Table dataSource={list} rowKey="id" columns={columns} loading={loading} />
            <Modal title={editing ? "ویرایش باشگاه" : "افزودن باشگاه"} open={visible} onOk={save} onCancel={() => setVisible(false)}>
                <Form form={form} layout="vertical">
                    <Form.Item label="نام" name="name" rules={[{ required: true }]}><Input /></Form.Item>
                    <Form.Item label="آدرس" name="address"><Input /></Form.Item>
                    <Form.Item label="تلفن" name="phone"><Input /></Form.Item>
                </Form>
            </Modal>
        </Card>
    );
};

export default GymsManagement;
