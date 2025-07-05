$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    $('#tblData').DataTable({
        ajax: {
            url: '/admin/product/getall',
            dataSrc: 'data' // This matches your { data: objProductList } structure
        },
        columns: [
            { data: 'title', width: '15%' },
            { data: 'author', width: '15%' },
            { data: 'isbn', width: '15%' },
            { data: 'listPrice', width: '10%' },
            { data: 'price', width: '10%' },
            { data: 'price50', width: '10%' },
            { data: 'price100', width: '10%' },
            {
                data: 'category.categoryName',
                width: '15%',
                defaultContent: 'N/A' // Shows when category is null
            },
            {
                data: 'productId',
                render: function (data) {
                    return `
            <div class="btn-group">
                <a href="/admin/product/upsert?id=${data}" class="btn btn-primary mx-2">
                    <i class="bi bi-pencil-square"></i> Edit
                </a>
                <a onclick="deleteProduct(${data})" class="btn btn-danger mx-2">
                    <i class="bi bi-trash-fill"></i> Delete
                </a>
            </div>`;
                }
            }
        ],
        initComplete: function () {
            console.log('DataTable initialized'); // Debugging
        },
        error: function (xhr, error, thrown) {
            console.error('Error loading data:', error, thrown);
        }
    });
}


function deleteProduct(productId) {
    Swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, delete it!'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: '/admin/product/delete',
                type: 'POST',
                data: { id: productId },
                headers: {
                    "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val()
                },
                success: function (response) {
                    if (response.success) {
                        Swal.fire('Deleted!', response.message, 'success')
                            .then(() => location.reload()); // Full page reload
                    } else {
                        Swal.fire('Error!', response.message, 'error');
                    }
                },
                error: function () {
                    Swal.fire('Error!', 'Failed to delete product', 'error');
                }
            });
        }
    });
}