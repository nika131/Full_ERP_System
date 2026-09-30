import LegalPage from './LegalPage';

export default function PrivacyPolicy() {
    return (
        <LegalPage title="Privacy Policy">
            <p className="text-sm text-gray-500 mb-6">
                TenexERP · Last updated: September 30, 2026
            </p>

            <p>
                TenexERP ("the App") is a point-of-sale (POS) application for
                recording sales. It is operated by TenexERP, an independent
                developer based in Georgia ("we", "us"). This policy explains
                what data we collect, how we use it, and the choices you have.
                It covers the mobile app and the web dashboard that goes with it.
            </p>

            <p className="mt-4">
                The mobile app is used to sign in and record sales. It does not
                offer sign-up or account deletion. Accounts are created and
                managed through our web dashboard.
            </p>

            <section className="mt-8">
                <h2 className="text-xl font-semibold text-gray-900 mb-3">
                    1. Data we collect
                </h2>

                <h3 className="font-semibold text-gray-900 mt-5 mb-2">
                    Account data
                </h3>

                <ul className="list-disc pl-6 space-y-1">
                    <li>Username</li>
                    <li>Email address</li>
                    <li>Password (stored only in hashed form, never in plain text)</li>
                </ul>

                <p className="mt-3">
                    We do not collect your name, age, phone number, home address,
                    government ID, or other personal identifiers.
                </p>

                <h3 className="font-semibold text-gray-900 mt-5 mb-2">
                    Business data you enter
                </h3>

                <ul className="list-disc pl-6 space-y-1">
                    <li>Products, categories, prices, and stock quantities</li>
                    <li>Store names and locations</li>
                    <li>
                        Sales and receipts (items, quantities, discounts, totals, payment method such as cash or card, and timestamps)
                    </li>
                    <li>
                        Employee salary records entered by authorized users in
                        the web dashboard
                    </li>
                </ul>

                <h3 className="font-semibold text-gray-900 mt-5 mb-2">
                    Activity logs
                </h3>

                <p>
                    We keep an audit log of actions taken in the system, for
                    example which user made a sale, or changed a product name,
                    category, price, or salary record, and when. This supports accountability and security within your business.
                </p>

                <h3 className="font-semibold text-gray-900 mt-5 mb-2">
                    Location (web dashboard only)
                </h3>

                <p>
                    The web dashboard asks for your browser's location
                    permission only when you add a new store using the map.
                    The mobile app does not request any device permissions and does
                    not access your location, camera, contacts, microphone, or
                    files. Location is used only to place the store on the map. The store's coordinates are then kept as business data.
                </p>

                <h3 className="font-semibold text-gray-900 mt-5 mb-2">
                    Payments
                </h3>

                <p>
                    The App records the payment method chosen for a sale (for example cash or card). It does not process card payments, and we
                    do not collect or store card numbers or bank details.
                </p>
            </section>

            <section className="mt-8">
                <h2 className="text-xl font-semibold text-gray-900 mb-3">
                    2. How we use the data
                </h2>

                <ul className="list-disc pl-6 space-y-1">
                    <li>
                        To create and secure your account and let you sign in
                    </li>
                    <li>
                        To provide the core features: sales, products, stores, reports, and salary records
                    </li>
                    <li>
                        To keep audit logs of changes
                    </li>
                    <li>
                        To maintain, secure, and fix the service
                    </li>
                </ul>

                <p className="mt-3">
                    We do not sell your data, use it for advertising, or share
                    it with advertisers or data brokers.
                </p>
            </section>

            <section className="mt-8">
                <h2 className="text-xl font-semibold text-gray-900 mb-3">
                    3. Who can see the data
                </h2>

                <p>
                    Data is visible to users within the same business according
                    to their permissions. For example, dashboard users can add other users by email, assign them to stores, and view sales and activity logs. Business owners and administrators
                    are responsible for the data they and their staff enter,
                    including employee salary information.
                </p>
            </section>

            <section className="mt-8">
                <h2 className="text-xl font-semibold text-gray-900 mb-3">
                    4. Where data is stored
                </h2>

                <p>
                    Data is stored on Microsoft Azure (Poland Central region)
                    and sent over encrypted HTTPS connections. Microsoft acts
                    as our hosting provider. We do not share data with other
                    third parties, except where required by law.
                </p>
            </section>

            <section className="mt-8">
                <h2 className="text-xl font-semibold text-gray-900 mb-3">
                    5. Retention
                </h2>

                <p>
                    We keep your account and business data while your account
                    is active. When your account is deleted, we delete or
                    anonymize your personal data (username, email, credentials) within 30 days. Sales records
                    and audit logs may be kept in anonymized form, or for as
                    long as accounting or legal obligations require.
                </p>
            </section>

            <section className="mt-8">
                <h2 className="text-xl font-semibold text-gray-900 mb-3">
                    6. Your rights, account creation and deletion
                </h2>

                <p>
                    Accounts are created and managed through the TenexERP web
                    dashboard, or by emailing{' '}
                    <a
                        href="mailto:support@tenexerp.com"
                        className="text-emerald-600 hover:underline"
                    >
                        support@tenexerp.com
                    </a>{' '}
                    from your registered address. We will confirm and process deletion requests within 30 days.
                </p>

                <p className="mt-3">
                    You may also request access to, correction of, or deletion of
                    your data at any time by emailing{' '}
                    <a
                        href="mailto:support@tenexerp.com"
                        className="text-emerald-600 hover:underline"
                    >
                        support@tenexerp.com
                    </a>. If you are in the EU/EEA, you also have the right to data portability, to object to or restrict processing, and to lodge a complaint with your local data protection authority.
                </p>
            </section>

            <section className="mt-8">
                <h2 className="text-xl font-semibold text-gray-900 mb-3">
                    7. Security
                </h2>

                <p>
                    We use hashed passwords, encrypted connections, and access
                    controls. No system is completely secure, but we work to
                    protect your data.
                </p>
            </section>

            <section className="mt-8">
                <h2 className="text-xl font-semibold text-gray-900 mb-3">
                    8. Children
                </h2>

                <p>
                    The App is intended for businesses and is not directed at
                    children under 16 (or the applicable age in your country).
                    We do not knowingly collect data from children.
                </p>
            </section>

            <section className="mt-8">
                <h2 className="text-xl font-semibold text-gray-900 mb-3">
                    9. Changes to this policy
                </h2>

                <p>
                    We may update this policy from time to time. The "Last
                    updated" date shows the latest version, and we will notify
                    users of material changes.
                </p>
            </section>

            <section className="mt-8">
                <h2 className="text-xl font-semibold text-gray-900 mb-3">
                    10. Terms of Service
                </h2>

                <p>
                    Your use of TenexERP is also governed by our{' '}
                    <a
                        href="/terms-of-service"
                        className="text-emerald-600 hover:underline"
                    >
                        Terms of Service
                    </a>, including disclaimers and limitations of liability.
                </p>
            </section>

            <section className="mt-8">
                <h2 className="text-xl font-semibold text-gray-900 mb-3">
                    11. Contact
                </h2>

                <p>
                    TenexERP
                    <br />
                    General:{' '}
                    <a
                        href="mailto:contact@tenexerp.com"
                        className="text-emerald-600 hover:underline"
                    >
                        contact@tenexerp.com
                    </a>
                    <br />
                    Support and privacy requests:{' '}
                    <a
                        href="mailto:support@tenexerp.com"
                        className="text-emerald-600 hover:underline"
                    >
                        support@tenexerp.com
                    </a>
                    <br />
                    Information:{' '}
                    <a
                        href="mailto:info@tenexerp.com"
                        className="text-emerald-600 hover:underline"
                    >
                        info@tenexerp.com
                    </a>
                </p>
            </section>
        </LegalPage>
    );
}