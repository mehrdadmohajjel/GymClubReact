import React, { useEffect, useState } from "react";
import { Card, Table, Button, Modal, Form, Input, message } from "antd";
import api from "../../api/axios";

const Movements: React.FC = () => {
    const [list, setList] = useState<any[]>([]);
    const [visible, setVisible] = useState(false);
    const [editing, setEditing] = useState<any>(null);
    const [form] = Form.useForm();

    const load = async () => {
        const res = await api.get("/movements/list");
        setList(res.data || []);
    };

    useEffect(() => { load(); }, []);

    const save = async () => {
        const vals = await form.validateFields();
        try {
            if (editing) await api.put(`/movements/${editing.id}`, vals);
            else await api.post("/movements/create", vals);
            message.success("ذخیره شد");
            setVisible(false);
            load();
        } catch (err: any) { message.error(err?.response?.data?.error ?? "خطا"); }
    };

    return (
        <Card title="حرکات">
            <Button onClick={() => { setEditing(null); form.resetFields(); setVisible(true); }}>افزودن حرکت</Button>
            <Table dataSource={list} rowKey="id" style={{ marginTop: 12 }} columns={[
                { title: "نام", dataIndex: "name" },
                { title: "ویدیو", dataIndex: "videoUrl", render: (v: any) => v ? <a href={v} target="_blank">مشاهده</a> : "ندارد" },
                { title: "عملیات", render: (_: any, r: any) => <Button onClick={() => { setEditing(r); form.setFieldsValue(r); setVisible(true); }}>ویرایش</Button> }
            ]} />
            <Modal title="حرکت" open={visible} onOk={save} onCancel={() => setVisible(false)}>
                <Form form={form} layout="vertical">
                    <Form.Item name="name" label="نام" rules={[{ required: true }]}><Input /></Form.Item>
                    <Form.Item name="videoUrl" label="لینک ویدیو"><Input /></Form.Item>
                </Form>
            </Modal>
        </Card>
    );
};

export default Movements;
