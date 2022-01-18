var dataTable;

$(document).ready(function(){
    loadDataTable();
})

function loadDataTable() {

    dataTable = $('#tblData').DataTable({

        "ajax": {
            "url": "/Admin/Company/GetAll"
        },

        "columns": [
            { "data": "name", "width": "15%", "className": "dt-center" },
            { "data": "streetAddress", "width": "15%", "className": "dt-center" },
            { "data": "city", "width": "15%", "className": "dt-center" },
            { "data": "state", "width": "15%", "className": "dt-center" },
            { "data": "phoneNumber", "width": "15%", "className": "dt-center" },
            {
                "data": "isAuthorizedCompany", "width": "10%", "className": "dt-center",
                "render": function (data) {
                    if (data) {
                        return `<input type="checkbox" disabled checked />`;
                    }
                    else
                    {
                        return `<input type="checkbox" disabled  />`;
                    }
                }
            },
            {
                "data": "id",
                "render": function (data) {
                    return `<div class="text-center">
                      <a href="/Admin/Company/Upsert/${data}" class="btn btn-info"><i class="far fa-edit"></i></a>
                      <a onclick=Delete("/Admin/Company/Delete/${data}") class="btn btn-danger"><i class="fas fa-trash-alt"></i></a>
               
                         </div> `;
                }

            }
        ]
    })
}

function Delete(url) {
    
    swal({
        title: "Want to delete record?",
        text: "Information Deleted Can not be recoverd",
        icon: "warning",
        buttons: true,
        dangerModel:true
    }).then((willdelete) => {
        if (willdelete) {

            $.ajax({
                url: url,
                type: "DELETE",
                success: function (data) {
                    if (data.success) {
                        toastr.success(data.message);
                        dataTable.ajax.reload();
                    }
                    else {
                        toastr.error(data.message);
                    }

                }
            })
           
        }
    })
}