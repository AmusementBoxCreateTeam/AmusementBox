<?php $this->load->view('common/header'); ?>
<?php echo form_open('statistic/rank5'); ?>
<div class="container-fluid">
    <div class="panel panel-info">
        <div class="panel-heading">
            <div class="panel-title">ランキングTOP5</div>
        </div>
        <div class="panel-body">
            <select name="gender">
                <option value="" <?php echo set_select('gender', ''); ?> >男女の選択</option>
                <option value="1" <?php echo set_select('gender', '1'); ?>>男性</option>
                <option value="0" <?php echo set_select('gender', '0'); ?>>女性</option>
            </select>
            <select name="age">
                <option valeu="" <?php echo set_select('age', ''); ?>>年代の選択</option>
                <option value="10" <?php echo set_select('age', '10'); ?>>10代</option>
                <option value="20" <?php echo set_select('age', '20'); ?>>20代</option>
                <option value="30" <?php echo set_select('age', '30'); ?>>30代</option>
                <option value="40" <?php echo set_select('age', '40'); ?>>40代</option>
                <option value="50" <?php echo set_select('age', '50'); ?>>50代</option>
                <option value="60" <?php echo set_select('age', '60'); ?>>60代</option>
                <option value="70" <?php echo set_select('age', '70'); ?>>70代</option>
                <option value="80" <?php echo set_select('age', '80'); ?>>80代</option>
                <option value="90" <?php echo set_select('age', '90'); ?>>90代</option>
            </select>
            </form>
        </div>
        <div class="text-center" style="margin-bottom:10px;"><button type="submit" class="btn btn-info btn-lg"><i class="glyphicon glyphicon-search"></i>検索</button></div>
        <div>
            <?php if (!empty($list)) { ?>
            <?php } ?>
        </div>
    </div>
</div>
<?php $this->load->view('common/footer'); ?>
