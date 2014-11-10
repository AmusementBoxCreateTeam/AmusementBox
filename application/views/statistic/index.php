<?php $this->load->view('common/header'); ?>
<?php echo form_open(); ?>
<select>
    <option>9歳以下</option>
    <option>10代</option>
    <option>20代</option>
    <option>30代</option>
    <option>40代</option>
    <option>50代</option>
    <option>60代</option>
    <option>70代</option>
    <option>80代</option>
    <option>90歳以上</option>
</select>
<select>
    <option>男</option>
    <option>女</option>
</select>
</form>
<?php if (!empty($list)) { ?>
<?php } ?>
<?php $this->load->view('common/footer'); ?>
