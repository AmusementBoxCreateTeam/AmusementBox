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
	                    <th>経度</th>
	                    <td>
	                    	<input type="text" name="x" value="<?php echo set_value('x'); ?>" />
	                    </td>
	                </tr>
	                <tr>
	                    <th>緯度</th>
	                    <td>
	                    	<input type="text" name="y" value="<?php echo set_value('y'); ?>" />
	                    </td>
	                </tr>
	                <tr>
	                	<th>設置都道府県</th>
	                	<td>
	                		<select name="prefectures">
		                	<?php
		                		foreach ($pref_list as $value) {
		                			$selected = $value == set_value('prefectures') ? " selected" : "";
		                	?>
		                		<option <?php echo $selected; ?>><?php echo $value; ?></option>
		                	<?php
		                		}
		                	?>
	                		</select>
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
