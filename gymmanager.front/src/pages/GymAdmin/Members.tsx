import React, { useEffect, useState } from "react";
import { Card, Table, Button, Modal, Form, Input, message, Pagination } from "antd";
import api from "../../api/axios";

const Members: React.FC = () => {
    const [data, setData] = useState<any[]>([]);
    const [loading, setLoading] = useState(false);
    const [visible, setVisible] = useState(false);
    const [editing, setEditing] = useState<any>(null);
    const [form] = Form.useForm();
    const [page, setPage] = useState(1);

    const load = async (p = 1) => {
        setLoading(true);
        try {
            const res = await api.get(`/users/list?page=${p}&pageSize=20`);
            setData(res.data.data || []);
        } finally { setLoading(false); }
    };

    useEffect(() => { load(); }, []);

    const openAdd = () => { setEditing(null); form.resetFields(); setVisible(true); };
    const openEdit = (record: any) => { setEditing(record); form.setFieldsValue(record); setVisible(true); };

    const save = async () => {
        const vals = await form.validateFields();
        try {
            if (editing) {
                await api.put(`/users/${editing.id}`, vals);
                message.success("ویرایش شد");
            } else {
                await api.post("/users/create", { ...vals, password: vals.password });
                message.success("افزودن موفق");
            }
            setVisible(false);
            load();
        } catch (err: any) {
            message.error(err?.response?.data?.error ?? "خطا");
        }
    };

    const columns = [
        { title: "نام", dataIndex: "firstName" },
        { title: "نام خانوادگی", dataIndex: "lastName" },
        { title: "کد ملی", dataIndex: "nationalCode" },
        {
            title: "عملیات", render: (_: any, r: any) => (
                <>
                    <Button onClick={() => openEdit(r)} size="small">ویرایش</Button>
                </>
            )
        }
    ];

    return (
        <Card title="مدیریت اعضا" extra={<Button onClick={openAdd}>افزودن عضو</Button>}>
            <Table rowKey="id" dataSource={data} columns={columns} loading={loading} pagination={false} />
            <Pagination style={{ marginTop: 12 }} current={page} onChange={(p) => { setPage(p); load(p) }} />
            <Modal title={editing ? "ویرایش" : "افزودن عضو"} open={visible} onOk={save} onCancel={() => setVisible(false)}>
                <Form form={form} layout="vertical">
                    <Form.Item label="نام" name="firstName" rules={[{ required: true }]}><Input /></Form.Item>
                    <Form.Item label="نام خانوادگی" name="lastName" rules={[{ required: true }]}><Input /></Form.Item>
                    <Form.Item label="کد ملی" name="nationalCode" rules={[{ required: true }]}><Input /></Form.Item>
                    <Form.Item label="شماره" name="phone"><Input /></Form.Item>
                    {!editing && <Form.Item label="رمز عبور" name="password" rules={[{ required: true }]}><Input.Password /></Form.Item>}
                </Form>
            </Modal>
        </Card>
    );
};

export default Members;
