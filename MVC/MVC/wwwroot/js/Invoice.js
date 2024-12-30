const mockInvoices = [
    {
        "Id": 3,
        "InvoiceNumber": "INV-54321",
        "InvoiceDate": "2024-11-18",
        "CustomerName": "Alice Johnson",
        "CustomerEmail": "alicejohnson@example.com",
        "CustomerPhone": "321-654-9870",
        "CustomerAddress": "789 Pine Road, Greenville",
        "Subtotal": 1200,
        "TaxAmount": 120,
        "TotalAmount": 1320,
        "Status": "Overdue",
        "Notes": "Urgent payment required. Please contact us for any issues."
    },
    {
        "Id": 4,
        "InvoiceNumber": "INV-11223",
        "InvoiceDate": "2024-11-10",
        "CustomerName": "Bob Brown",
        "CustomerEmail": "bobbrown@example.com",
        "CustomerPhone": "654-321-9876",
        "CustomerAddress": "101 Maple Lane, Rivertown",
        "Subtotal": 450,
        "TaxAmount": 45,
        "TotalAmount": 495,
        "Status": "Paid",
        "Notes": "Payment received on time. Thank you!"
    },
    {
        "Id": 5,
        "InvoiceNumber": "INV-33445",
        "InvoiceDate": "2024-11-25",
        "CustomerName": "Charlie Green",
        "CustomerEmail": "charliegreen@example.com",
        "CustomerPhone": "555-666-7777",
        "CustomerAddress": "202 Birch Boulevard, Lake City",
        "Subtotal": 800,
        "TaxAmount": 80,
        "TotalAmount": 880,
        "Status": "Pending",
        "Notes": "Payment due within 15 days."
    },
    {
        "Id": 6,
        "InvoiceNumber": "INV-88990",
        "InvoiceDate": "2024-11-12",
        "CustomerName": "David White",
        "CustomerEmail": "davidwhite@example.com",
        "CustomerPhone": "888-999-0000",
        "CustomerAddress": "303 Cedar Circle, Hilltop",
        "Subtotal": 1500,
        "TaxAmount": 150,
        "TotalAmount": 1650,
        "Status": "Paid",
        "Notes": "Paid with credit card."
    },
    {
        "Id": 7,
        "InvoiceNumber": "INV-77654",
        "InvoiceDate": "2024-11-22",
        "CustomerName": "Eve Black",
        "CustomerEmail": "eveblack@example.com",
        "CustomerPhone": "444-555-6666",
        "CustomerAddress": "404 Willow Way, Coastal Town",
        "Subtotal": 700,
        "TaxAmount": 70,
        "TotalAmount": 770,
        "Status": "Pending",
        "Notes": "Payment expected within 7 days."
    }
];

// Event Listener for Search Button
document.getElementById('searchInvoice').addEventListener('click', () => {
    const inputInvoiceNumber = document.getElementById('invoiceNumberInput').value.trim();
    const invoice = mockInvoices.find(inv => inv.InvoiceNumber === inputInvoiceNumber);

    if (invoice) {
        displayInvoice(invoice);
    } else {
        // Show error message if invoice not found
        document.getElementById('invoiceDisplay').classList.add('d-none');
        document.getElementById('errorMessage').classList.remove('d-none');
    }
});

// Function to Display Invoice
function displayInvoice(invoice) {
    // Hide error message and show invoice
    document.getElementById('errorMessage').classList.add('d-none');
    document.getElementById('invoiceDisplay').classList.remove('d-none');

    document.getElementById('invoiceNumber').innerText = invoice.InvoiceNumber;
    document.getElementById('invoiceDate').innerText = new Date(invoice.InvoiceDate).toLocaleDateString();
    document.getElementById('customerName').innerText = invoice.CustomerName;
    document.getElementById('customerEmail').innerText = invoice.CustomerEmail;
    document.getElementById('customerPhone').innerText = invoice.CustomerPhone;
    document.getElementById('customerAddress').innerText = invoice.CustomerAddress;
    document.getElementById('subtotal').innerText = invoice.Subtotal.toFixed(2);
    document.getElementById('taxAmount').innerText = invoice.TaxAmount.toFixed(2);
    document.getElementById('totalAmount').innerText = invoice.TotalAmount.toFixed(2);
    document.getElementById('status').innerText = invoice.Status;
    document.getElementById('notes').innerText = invoice.Notes;
}