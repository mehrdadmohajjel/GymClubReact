import React, { useEffect, useState } from "react";
import { Card, List } from "antd";
import api from "../../api/axios";

const AthleteDashboard: React.FC = () => {
    const [membership, setMembership] = useState<any>(null);
    const [programs, setPrograms] = useState<any[]>([]);

    useEffect(() => {
        (async () => {
            const m = await api.get("/memberships/my");
            setMembership(m.data[0] ?? null);
            const w = await api.get("/workouts/my");
            setPrograms(w.data || []);
        })();
    }, []);

    return (
        <div>
            <Card title="وضعیت عضویت">
                {membership ? <div>نوع: {membership.type} - جلسات باقیمانده: {membership.remainingSessions || 0}</div> : <div>عضویتی ندارید</div>}
            </Card>
            <Card title="برنامه‌ها" style={{ marginTop: 12 }}>
                <List dataSource={programs} renderItem={(p: any) => <List.Item>{p.title}</List.Item>} />
            </Card>
        </div>
    );
};

export default AthleteDashboard;
