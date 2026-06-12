import { useState, useEffect } from 'react';
import toast from 'react-hot-toast';
import { authService } from '../../api/authService';
import apiClient from '../../api/apiClient';

interface InviteModalProps {
    onClose: () => void;
    onSuccess: () => void;
}

export const InviteEmployeeModal = ({ onClose, onSuccess }: InviteModalProps) => {
    const [formData, setFormData] = useState({
        fullName: '',
        username: '',
        password: '',
        roleId: 0
    });
    const [roles, setRoles] = useState<{ roleId: number, name: string }[]>([]);
    const [isLoading, setIsLoading] = useState(false);

    useEffect(() => {
        apiClient.get('/roles/lookup')
            .then(res => setRoles(res.data))
            .catch(() => toast.error("Failed to load roles."));
    }, []);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setIsLoading(true);

        try {
            await authService.register({
                ...formData,
                roleId: Number(formData.roleId)
            });
            
            toast.success("Employee created successfully.");
            onSuccess();
        } catch (error: any) {
            toast.error(error.response?.data?.message || "Failed to create employee.");
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <div className="fixed inset-0 bg-black/50 z-50 flex items-center justify-center p-4">
            <div className="bg-white rounded-lg shadow-xl w-full max-w-md p-6">
                <h3 className="text-lg font-bold text-slate-800 mb-4">Invite New Employee</h3>
                <form onSubmit={handleSubmit} className="space-y-4">
                    <div>
                        <label className="block text-sm font-medium text-slate-700">Full Name</label>
                        <input 
                            type="text" required
                            className="mt-1 w-full p-2 border border-slate-300 rounded-md outline-none focus:border-emerald-500"
                            value={formData.fullName}
                            onChange={e => setFormData({...formData, fullName: e.target.value})}
                        />
                    </div>
                    <div>
                        <label className="block text-sm font-medium text-slate-700">Username</label>
                        <input 
                            type="text" required
                            className="mt-1 w-full p-2 border border-slate-300 rounded-md outline-none focus:border-emerald-500"
                            value={formData.username}
                            onChange={e => setFormData({...formData, username: e.target.value})}
                        />
                    </div>
                    <div>
                        <label className="block text-sm font-medium text-slate-700">Password</label>
                        <input 
                            type="password" required
                            className="mt-1 w-full p-2 border border-slate-300 rounded-md outline-none focus:border-emerald-500"
                            value={formData.password}
                            onChange={e => setFormData({...formData, password: e.target.value})}
                        />
                    </div>
                    <div>
                        <label className="block text-sm font-medium text-slate-700">System Role</label>
                        <select 
                            required
                            className="mt-1 w-full p-2 border border-slate-300 rounded-md outline-none focus:border-emerald-500"
                            value={formData.roleId}
                            onChange={e => setFormData({...formData, roleId: Number(e.target.value)})}
                        >
                            <option value={0}>Select a role...</option>
                            {roles.map(r => (
                                <option key={r.roleId} value={r.roleId}>{r.name}</option>
                            ))}
                        </select>
                    </div>

                    <div className="flex justify-end gap-2 pt-4">
                        <button 
                            type="button" 
                            onClick={onClose} 
                            className="px-4 py-2 text-sm text-slate-600 hover:bg-slate-100 rounded-md"
                        >
                            Cancel
                        </button>
                        <button 
                            type="submit" 
                            disabled={isLoading} 
                            className="px-4 py-2 text-sm bg-emerald-600 text-white rounded-md hover:bg-emerald-700 disabled:opacity-50"
                        >
                            {isLoading ? 'Creating...' : 'Create Employee'}
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
};