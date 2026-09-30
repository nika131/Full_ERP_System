import { useState } from "react";
import { useAuth } from "../context/AuthContext";
import { Link, useNavigate } from "react-router-dom";
import { authService } from "../api/authService";

export default function Login() {
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState('');
    const [isLoading, setIsLoading] = useState(false);

    const navigate = useNavigate();
    const { login } = useAuth();

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setError('');
        setIsLoading(true);

        try {
            const response = await authService.login({ username, password });
            login(response.data.token);
            navigate('/profile');
        } catch (err: any) {
            setError('Invalid username or password');
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <div className="min-h-screen flex items-center justify-center bg-gray-100 px-4 py-8">
            <div className="w-full max-w-md">

                {/* LOGIN FORM */}
                <div className="bg-white p-6 sm:p-8 rounded-lg shadow-md w-full border border-gray-200">
                    <div className="text-center mb-8">
                        <h1 className="text-2xl font-bold text-gray-900">
                            Nexus ERP
                        </h1>

                        <p className="text-sm text-gray-500">
                            Sign in to your account
                        </p>
                    </div>

                    {error && (
                        <div className="bg-red-50 text-red-600 p-3 rounded mb-4 text-sm font-medium border border-red-200">
                            {error}
                        </div>
                    )}

                    <form onSubmit={handleSubmit} className="space-y-6">
                        <div>
                            <label className="block text-sm font-medium text-gray-700 mb-1">
                                Username
                            </label>

                            <input
                                type="text"
                                value={username}
                                onChange={(e) => setUsername(e.target.value)}
                                className="w-full px-4 py-2 border border-gray-300 rounded focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none transition"
                                required
                            />
                        </div>

                        <div>
                            <label className="block text-sm font-medium text-gray-700 mb-1">
                                Password
                            </label>

                            <input
                                type="password"
                                value={password}
                                onChange={(e) => setPassword(e.target.value)}
                                className="w-full px-4 py-2 border border-gray-300 rounded focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none transition"
                                required
                            />
                        </div>

                        <button
                            type="submit"
                            disabled={isLoading}
                            className="w-full bg-blue-600 hover:bg-blue-700 text-white font-semibold py-2 px-4 rounded transition disabled:opacity-50"
                        >
                            {isLoading ? 'Authenticating...' : 'Sign In'}
                        </button>
                    </form>
                </div>

                {/* LINKS */}
                <div className="mt-6 text-center text-sm text-gray-500">
                    <div className="flex flex-wrap justify-center gap-x-5 gap-y-2">
                        <Link
                            to="/privacy-policy"
                            className="hover:text-emerald-600"
                        >
                            Privacy Policy
                        </Link>

                        <Link
                            to="/terms-of-service"
                            className="hover:text-emerald-600"
                        >
                            Terms of Service
                        </Link>

                        <a
                            href="mailto:support@tenexerp.com"
                            className="hover:text-emerald-600"
                        >
                            Support
                        </a>
                    </div>

                    <p className="mt-3">
                        © {new Date().getFullYear()} TenexERP
                    </p>
                </div>

            </div>
        </div>
    );
}