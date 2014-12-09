<?php $this->load->view('common/header'); ?>

<div class="container-fluid">
    <h2>端末登録</h2>
    <?php
    $attributes = array('class' => 'form-inline', 'role' => 'form');
    echo form_open('box/register', $attributes);
    ?>
	    <?php echo validation_errors(); ?>
	    <div class="panel panel-info">
	        <div class="panel-heading">
	            <div class="panel-title">端末登録</div>
	        </div>
	        <div class="panel-body">
	            <table class="table table-bordered">
	                <tr>
	                	<th>設置住所</th>
	                	<td>
	                		<input type="text" name="address" value="<?php echo set_value('address'); ?>" />
	                	</td>
	                </tr>
	            </table>
	        </div>
	        <div class="text-center" style="margin-bottom:10px;">
	        	<button type="submit" class="btn btn-info btn-lg"><i class="glyphicon glyphicon-search" onclick="search_user"></i>登録</button>
	        </div>
	    </div>
	</form>
</div>

<?php $this->load->view('common/footer'); ?>
